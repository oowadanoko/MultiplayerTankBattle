using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgGetAchieve : MsgBase
{
    public int win = 0;
    public int lost = 0;

    public MsgGetAchieve()
    {
        protoName = "MsgGetAchieve";
    }
}

[System.Serializable]
public class RoomInfo
{
    public int id = 0;
    public int count = 0;
    public int status = 0;
}

public class MsgGetRoomList : MsgBase
{
    public RoomInfo[] rooms;

    public MsgGetRoomList()
    {
        protoName = "MsgGetRoomList";
    }
}

public class MsgCreateRoom : MsgBase
{
    public int result = 0;
    public MsgCreateRoom()
    {
        protoName = "MsgCreateRoom";
    }
}

public class MsgEnterRoom : MsgBase
{
    public int result = 0;
    public int id = 0;

    public MsgEnterRoom()
    {
        protoName = "MsgEnterRoom";
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string id = "lpy";
    public int camp = 0;
    public int win = 0;
    public int lost = 0;
    public int isOwner = 0;
}


public class MsgGetRoomInfo : MsgBase
{
    public PlayerInfo[] players;

    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }
}

public class MsgLeaveRoom : MsgBase
{
    public int result = 0;

    public MsgLeaveRoom()
    {
        protoName = "MsgLeaveRoom";
    }
}

public class MsgStartBattle : MsgBase
{
    public int result = 0;

    public MsgStartBattle()
    {
        protoName = "MsgStartBattle";
    }
}
