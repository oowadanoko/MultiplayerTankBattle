using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class MsgHandler
{
    public static void MsgGetAchieve(ClientState c, MsgBase msgBase)
    {
        MsgGetAchieve msg = msgBase as MsgGetAchieve;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        msg.win = player.data.win;
        msg.lost = player.data.lost;
        player.Send(msg);
    }

    public static void MsgGetRoomList(ClientState c, MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        player.Send(RoomManager.ToMsg());
    }

    public static void MsgCreateRoom(ClientState c, MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        if (player.roomId >= 0)
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        Room room = RoomManager.AddRoom();
        room.AddPlayer(player.id);
        msg.result = 0;
        player.Send(msg);
    }

    public static void MsgEnterRoom(ClientState c, MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        if (player.roomId >= 0)
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        Room room = RoomManager.GetRoom(msg.id);
        if (room == null)
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        if (!room.AddPlayer(player.id))
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        msg.result = 0;
        player.Send(msg);
    }

    public static void MsgGetRoomInfo(ClientState c, MsgBase msgBase)
    {
        MsgGetRoomInfo msg = msgBase as MsgGetRoomInfo;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            player.Send(msg);
            return;
        }
        player.Send(room.ToMsg());
    }

    public static void MsgLeaveRoom(ClientState c, MsgBase msgBase)
    {
        MsgLeaveRoom msg = msgBase as MsgLeaveRoom;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        room.RemovePlayer(player.id);
        msg.result = 0;
        player.Send(msg);
    }

    public static void MsgStartBattle(ClientState c, MsgBase msgBase)
    {
        MsgStartBattle msg = msgBase as MsgStartBattle;
        Player player = c.player;
        if (player == null)
        {
            return;
        }
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        if (!room.IsOwner(player))
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        if (!room.StartBattle())
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }
        msg.result = 0;
        player.Send(msg);
    }
}

