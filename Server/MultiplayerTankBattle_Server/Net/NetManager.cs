using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

public class NetManager
{
    public static Socket listenfd;
    public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
    static List<Socket> checkRead = new List<Socket>();//检查可读性的列表
    public static long pingInterval = 30;//每次ping客户端的时间间隔

    /// <summary>
    /// 开始循环监听，要在外部调用
    /// </summary>
    /// <param name="listenPort">要监听的端口号</param>
    public static void StartLoop(int listenPort)
    {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAdr = IPAddress.Parse("0.0.0.0");
        IPEndPoint ipEp = new IPEndPoint(ipAdr, listenPort);
        listenfd.Bind(ipEp);
        listenfd.Listen(0);
        Console.WriteLine("[服务器]启动成功");

        while (true)
        {
            ResetCheckRead();
            Socket.Select(checkRead, null, null, 1000);//过滤剩下有消息可读的连接
            for (int i = checkRead.Count - 1; i >= 0; --i)
            {
                Socket s = checkRead[i];
                if (s == listenfd)//有新连接
                {
                    ReadListenfd(s);
                }
                else
                {
                    ReadClientfd(s);
                }
            }
            Timer();
        }
    }

    /// <summary>
    /// 重置检查列表，将所有客户端连接加入列表中
    /// </summary>
    public static void ResetCheckRead()
    {
        checkRead.Clear();
        checkRead.Add(listenfd);
        foreach (ClientState s in clients.Values)
        {
            checkRead.Add(s.socket);
        }
    }

    /// <summary>
    /// 有新连接请求
    /// </summary>
    /// <param name="listenfd">监听套接字</param>
    public static void ReadListenfd(Socket listenfd)
    {
        try
        {
            Socket clientfd = listenfd.Accept();
            Console.WriteLine("Accept " + clientfd.RemoteEndPoint.ToString());
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Accept fail " + ex.ToString());
        }
    }

    /// <summary>
    /// 客户端有新消息待读取
    /// </summary>
    /// <param name="clientfd">客户端连接套接字</param>
    public static void ReadClientfd(Socket clientfd)
    {
        ClientState state = clients[clientfd];
        ByteArray readBuff = state.readBuff;
        int count = 0;
        if (readBuff.remain <= 0)//没有剩余容量了
        {
            //读取数据后移动数据，全部移动到数组最前面
            OnReceiveData(state);
            readBuff.MoveBytes();
        }
        if (readBuff.remain <= 0)
        {
            Console.WriteLine("Receive fail, maybe msg length > buff capacity");
            Close(state);
            return;
        }

        try
        {
            count = clientfd.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Receive SocketException " + ex.ToString());
            Close(state);
            return;
        }

        if (count <= 0)//消息长度为0代表客户端断开连接
        {
            Console.WriteLine("Socket Close" + clientfd.RemoteEndPoint.ToString());
            Close(state);
            return;
        }

        readBuff.writeIdx += count;
        OnReceiveData(state);//尝试继续读取消息
        readBuff.CheckAndMoveBytes();
    }

    public static void Close(ClientState state)
    {
        MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
        object[] ob = { state };
        mei.Invoke(null, ob);
        state.socket.Close();
        clients.Remove(state.socket);
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="state">要读取的客户端</param>
    public static void OnReceiveData(ClientState state)
    {
        ByteArray readBuff = state.readBuff;
        if (readBuff.length <= 2)//长度不足一个16位数据
        {
            return;
        }
        Int16 bodyLength = readBuff.ReadInt16();
        if (readBuff.length < bodyLength)//当前到达的数据不完整
        {
            return;
        }
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Console.WriteLine("OnReceiveData MsgBase.DecodeName fail");
            Close(state);
            return;
        }
        readBuff.readIdx += nameCount;
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);//根据协议名称调用对应的处理方法
        object[] o = { state, msgBase };
        Console.WriteLine("Receive " + protoName);
        if (mi != null)
        {
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceiveData Invoke fail " + protoName);
        }
        if (readBuff.length > 2)//还有可读数据，继续读取
        {
            OnReceiveData(state);
        }
    }

    static void Timer()
    {
        MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
        object[] ob = { };
        mei.Invoke(null, ob);
    }

    public static void Send(ClientState cs, MsgBase msg)
    {
        if (cs == null)
        {
            return;
        }
        if (!cs.socket.Connected)
        {
            return;
        }
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        //以小端模式编码整数
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        try
        {
            cs.socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket Close on BeginSend " + ex.ToString());
        }
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}

