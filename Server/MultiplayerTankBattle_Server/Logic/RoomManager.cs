using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoomManager
{
    private static int maxId = 1;//房间号，每次分配一个房间后递增
    public static Dictionary<int, Room> rooms = new Dictionary<int, Room>();

    public static Room GetRoom(int id)
    {
        if (rooms.ContainsKey(id))
        {
            return rooms[id];
        }
        return null;
    }

    public static Room AddRoom()
    {
        ++maxId;
        Room room = new Room();
        room.id = maxId;
        rooms.Add(room.id, room);
        return room;
    }

    public static bool RemoveRoom(int id)
    {
        rooms.Remove(id);
        return true;
    }

    /// <summary>
    /// 将所有房间打包成房间列表信息
    /// </summary>
    /// <returns>房间列表信息</returns>
    public static MsgBase ToMsg()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        int count = rooms.Count;
        msg.rooms = new RoomInfo[count];
        int i = 0;
        foreach (Room room in rooms.Values)
        {
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.id = room.id;
            roomInfo.count = room.playerIds.Count;
            roomInfo.status = (int)room.status;
            msg.rooms[i] = roomInfo;
            ++i;
        }
        return msg;
    }

    /// <summary>
    /// 判断每个房间是否阵营胜利，需要在外界调用
    /// </summary>
    public static void Update()
    {
        foreach (Room room in rooms.Values)
        {
            room.Update();
        }
    }
}
