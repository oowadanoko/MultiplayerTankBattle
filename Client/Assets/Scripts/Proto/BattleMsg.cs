using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TankInfo
{
    public string id = "";
    public int camp = 0;
    public int hp = 0;
    public float x = 0;
    public float y = 0;
    public float z = 0;
    public float ex = 0;
    public float ey = 0;
    public float ez = 0;
}

public class MsgEnterBattle : MsgBase
{
    public TankInfo[] tanks;
    public int mapId = 1;

    public MsgEnterBattle()
    {
        protoName = "MsgEnterBattle";
    }
}

public class MsgBattleResult : MsgBase
{
    public int winCamp = 0;

    public MsgBattleResult()
    {
        protoName = "MsgBattleResult";
    }
}

public class MsgLeaveBattle : MsgBase
{
    public string id = "";

    public MsgLeaveBattle()
    {
        protoName = "MsgLeaveBattle";
    }
}