using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("Close");
        if (c.player != null)
        {
            int roomId = c.player.roomId;
            if (roomId >= 0)
            {
                Room room = RoomManager.GetRoom(roomId);
                room.RemovePlayer(c.player.id);
            }
            DbManager.UpdatePlayerData(c.player.id, c.player.data);
            PlayerManager.RemovePlayer(c.player.id);
        }
    }

    public static void OnTimer()
    {
        CheckPing();
        RoomManager.Update();
    }

    /// <summary>
    /// 检查客户端的ping消息，如果长时间没有收到ping消息认为客户端已经断开连接
    /// </summary>
    public static void CheckPing()
    {
        long timeNow = NetManager.GetTimeStamp();
        foreach (ClientState s in NetManager.clients.Values)
        {
            //超过ping间隔的4倍认为超时
            if (timeNow - s.lastPingTime > NetManager.pingInterval * 4)
            {
                Console.WriteLine("Ping Close" + s.socket.RemoteEndPoint.ToString());
                NetManager.Close(s);
                return;
            }
        }
    }
}

