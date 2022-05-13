using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgGetAchieve : MsgBase
{
    public int win { get; set; } = 0;
    public int lost { get; set; } = 0;

    public MsgGetAchieve()
    {
        protoName = "MsgGetAchieve";
    }
}

[System.Serializable]
public class RoomInfo
{
    public int id { get; set; } = 0;
    public int count { get; set; } = 0;
    public int status { get; set; } = 0;
}

public class MsgGetRoomList : MsgBase
{
    public RoomInfo[] rooms { get; set; }

    public MsgGetRoomList()
    {
        protoName = "MsgGetRoomList";
    }
}

public class MsgCreateRoom : MsgBase
{
    public int result { get; set; } = 0;
    public MsgCreateRoom()
    {
        protoName = "MsgCreateRoom";
    }
}

public class MsgEnterRoom : MsgBase
{
    public int result { get; set; } = 0;
    public int id { get; set; } = 0;

    public MsgEnterRoom()
    {
        protoName = "MsgEnterRoom";
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string id { get; set; } = "lpy";
    public int camp { get; set; } = 0;
    public int win { get; set; } = 0;
    public int lost { get; set; } = 0;
    public int isOwner { get; set; } = 0;
}


public class MsgGetRoomInfo : MsgBase
{
    public PlayerInfo[] players { get; set; }

    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }
}

public class MsgLeaveRoom : MsgBase
{
    public int result { get; set; } = 0;

    public MsgLeaveRoom()
    {
        protoName = "MsgLeaveRoom";
    }
}

public class MsgStartBattle : MsgBase
{
    public int result { get; set; } = 0;

    public MsgStartBattle()
    {
        protoName = "MsgStartBattle";
    }
}

