using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MsgHandler
{
    public static void MsgSyncTank(ClientState c, MsgBase msgBase)
    {
        MsgSyncTank msg = msgBase as MsgSyncTank;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }
        if (room.status != Room.Status.FIGHT)
        {
            return;
        }
        //两次消息之间移动距离过大，怀疑可能作弊
        if (Math.Abs(player.x - msg.x) > 5 ||
            Math.Abs(player.y - msg.y) > 5 ||
            Math.Abs(player.z - msg.z) > 5)
        {
            Console.WriteLine("疑似作弊" + player.id);
        }
        player.x = msg.x;
        player.y = msg.y;
        player.z = msg.z;
        player.ex = msg.ex;
        player.ey = msg.ey;
        player.ez = msg.ez;
        msg.id = player.id;
        room.Broadcast(msg);
    }

    public static void MsgFire(ClientState c, MsgBase msgBase)
    {
        MsgFire msg = msgBase as MsgFire;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }
        if (room.status != Room.Status.FIGHT)
        {
            return;
        }
        msg.id = player.id;
        room.Broadcast(msg);
    }

    public static void MsgHit(ClientState c, MsgBase msgBase)
    {
        MsgHit msg = msgBase as MsgHit;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        Player targetPlayer = PlayerManager.GetPlayer(msg.targetId);
        if (targetPlayer == null)
        {
            return;
        }
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }
        if (room.status != Room.Status.FIGHT)
        {
            return;
        }
        if (player.id != msg.id)
        {
            return;
        }
        int damage = 35;
        targetPlayer.hp -= damage;
        msg.id = player.id;
        msg.hp = player.hp;
        msg.damage = damage;
        room.Broadcast(msg);
    }
}
