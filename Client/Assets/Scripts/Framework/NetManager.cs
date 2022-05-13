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
    static Queue<ByteArray> writeQueue;//���Ͷ���

    public delegate void EventListener(string err);
    private static Dictionary<NetEvent, EventListener> 
        eventListeners = new Dictionary<NetEvent, EventListener>();

    static bool isConnecting = false;//�Ƿ���������
    static bool isClosing = false;//�Ƿ����ڹر�����

    public delegate void MsgListener(MsgBase msgBase);
    private static Dictionary<string, MsgListener>
        msgListeners = new Dictionary<string, MsgListener>();

    static List<MsgBase> msgList = new List<MsgBase>();//��Ϣ����
    static int msgCount = 0;
    readonly static int MAX_MESSAGE_FIRE = 10;//һ����������Ϣ��

    public static bool isUsePing = true;//�Ƿ�ʹ����������
    public static int pingInterval = 30;//����ping��Ϣ��ʱ����
    static float lastPingTime = 0;//�ϴη���ping��Ϣ��ʱ��
    static float lastPongTime = 0;//�ϴν��յ�pong��Ϣ��ʱ��

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
    /// �ַ��¼���ִ�и��¼�ע��Ļص�����
    /// </summary>
    /// <param name="netEvent">�¼�</param>
    /// <param name="err">����</param>
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
            if (count == 0)//��Ϣ����Ϊ0������������Ͽ�����
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

    //������Ϣ
    public static void OnReceiveData()
    {
        if (readBuff.length <= 2)//���Ȳ���һ��16λ����
        {
            return;
        }
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.length < bodyLength + 2)//��ǰ��������ݲ�����
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
            msgList.Add(msgBase);//����Ϣ��ӵ���Ϣ�����еȴ�����
        }
        ++msgCount;
        if (readBuff.length > 2)//������Ϣ���Լ�����ȡ
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
    /// ����Ƿ�����Ϣδ������δ�������Ϣ�ַ�����Ӧ�Ĵ���������
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
                if (msgList.Count > 0)//�����ݴ�����
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    --msgCount;
                }
            }
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);//������Ӧ�ĺ�������
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
        if (writeQueue.Count > 0)//����Ϣδ�����꣬��ǰ�������ڹر�״̬�����ܽ�����������������Ϣ��������ٹر�
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
        //��С��ģʽ��������
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock(writeQueue)
        {
            writeQueue.Enqueue(ba);//����Ϣд�뷢�Ͷ����еȴ�����
            count = writeQueue.Count;
        }
        if (count == 1)//ԭ�ȶ���Ϊ�գ�ֻ���¼ӵ�һ����Ϣ���Ͳ���Ҫ�ȴ���ֱ�ӷ���
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
        int count = socket.EndSend(ar);//���͵��ֽ���
        ByteArray ba;
        lock(writeQueue)
        {
            ba = writeQueue.First();//ȡ�����е�һ������
        }
        ba.readIdx += count;
        if (ba.length == 0)//��һ�������Ѿ�ȫ��������
        {
            lock(writeQueue)
            {
                writeQueue.Dequeue();//��һ�����ݿ��Գ���
                ba = writeQueue.First();//��ȡ��һ����Ϣ
            }
        }
        if (ba != null)//���������һ��������Ϣ
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }
        else if (isClosing)//����������ڹر�״̬��������Ϣ���������ˣ����Թر�����
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
    /// �ַ���Ϣ������Ϣ������Ӧ�ĺ�������
    /// </summary>
    /// <param name="msgName">��Ϣ��</param>
    /// <param name="msgBase">��Ϣ</param>
    public static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }
}
