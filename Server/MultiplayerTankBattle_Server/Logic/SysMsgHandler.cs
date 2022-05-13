using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MsgHandler
{
    public static void MsgPing(ClientState c, MsgBase msgBase)
    {
        Console.WriteLine("MsgPing");
        c.lastPingTime = NetManager.GetTimeStamp();
        //服务器收到ping消息给客户端回复一个pong消息
        MsgPong msgPong = new MsgPong();
        NetManager.Send(c, msgPong);
    }
}
