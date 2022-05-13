using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

public class ClientState
{
    public Socket socket;
    public ByteArray readBuff = new ByteArray();
    public long lastPingTime = NetManager.GetTimeStamp();//上次从该客户端收到ping消息的时间
    public Player player;
}
