using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public static class NetManager
{
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;//发送队列

    public delegate void EventListener(string err);
    private static Dictionary<NetEvent, EventListener> 
        eventListeners = new Dictionary<NetEvent, EventListener>();

    static bool isConnecting = false;//是否正在连接
    static bool isClosing = false;//是否正在关闭连接

    public delegate void MsgListener(MsgBase msgBase);
    private static Dictionary<string, MsgListener>
        msgListeners = new Dictionary<string, MsgListener>();

    static List<MsgBase> msgList = new List<MsgBase>();//消息队列
    static int msgCount = 0;
    readonly static int MAX_MESSAGE_FIRE = 10;//一次最大处理的消息数

    public static bool isUsePing = true;//是否使用心跳机制
    public static int pingInterval = 30;//发送ping消息的时间间隔
    static float lastPingTime = 0;//上次发送ping消息的时间
    static float lastPongTime = 0;//上次接收到pong消息的时间

    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -=  listener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }

    /// <summary>
    /// 分发事件，执行该事件注册的回调函数
    /// </summary>
    /// <param name="netEvent">事件</param>
    /// <param name="err">参数</param>
    public static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect fail, already connected!");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect fail, isConnecting!");
            return;
        }
        InitState();
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    private static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
        msgList = new List<MsgBase>();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    private static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ ");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }

    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            int count = socket.EndReceive(ar);
            if (count == 0)//消息长度为0，代表服务器断开连接
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;
            OnReceiveData();
            if (readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail " + ex.ToString());
        }
    }

    //接收消息
    public static void OnReceiveData()
    {
        if (readBuff.length <= 2)//长度不足一个16位数据
        {
            return;
        }
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.length < bodyLength + 2)//当前到达的数据不完整
        {
            return;
        }
        readBuff.readIdx += 2;
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        lock(msgList)
        {
            msgList.Add(msgBase);//将消息添加到消息队列中等待处理
        }
        ++msgCount;
        if (readBuff.length > 2)//还有消息可以继续读取
        {
            OnReceiveData();
        }
    }

    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    /// <summary>
    /// 检查是否有消息未处理，将未处理的消息分发给对应的处理函数处理
    /// </summary>
    private static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }
        for (int i = 0; i < MAX_MESSAGE_FIRE; ++i)
        {
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)//有数据待处理
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    --msgCount;
                }
            }
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);//交给对应的函数处理
            }
            else
            {
                break;
            }
        }
    }

    private static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }
        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Debug.Log("Ping Close");
            Close();
        }
    }

    public static void Close()
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }
        if (writeQueue.Count > 0)//有消息未处理完，当前处于正在关闭状态，不能进行其他操作，等消息处理完后再关闭
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }

    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
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
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock(writeQueue)
        {
            writeQueue.Enqueue(ba);//将消息写入发送队列中等待发送
            count = writeQueue.Count;
        }
        if (count == 1)//原先队列为空，只有新加的一条消息，就不需要等待，直接发送
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    public static void SendCallback(IAsyncResult ar)
    {
        Socket socket = ar.AsyncState as Socket;
        if (socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);//发送的字节数
        ByteArray ba;
        lock(writeQueue)
        {
            ba = writeQueue.First();//取出队列第一个数据
        }
        ba.readIdx += count;
        if (ba.length == 0)//第一个数据已经全部发完了
        {
            lock(writeQueue)
            {
                writeQueue.Dequeue();//第一个数据可以出队
                ba = writeQueue.First();//获取下一个消息
            }
        }
        if (ba != null)//如果还有下一个待发消息
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }
        else if (isClosing)//如果处于正在关闭状态，所有消息都发送完了，可以关闭连接
        {
            socket.Close();
        }
    }

    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
        }
    }

    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    /// <summary>
    /// 分发消息，将消息发给对应的函数处理
    /// </summary>
    /// <param name="msgName">消息名</param>
    /// <param name="msgBase">消息</param>
    public static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }
}
