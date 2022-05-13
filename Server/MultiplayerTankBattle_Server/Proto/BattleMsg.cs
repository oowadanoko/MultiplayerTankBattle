using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TankInfo
{
    public string id { get; set; } = "";
    public int camp { get; set; } = 0;
    public int hp { get; set; } = 0;
    public float x { get; set; } = 0;
    public float y { get; set; } = 0;
    public float z { get; set; } = 0;
    public float ex { get; set; } = 0;
    public float ey { get; set; } = 0;
    public float ez { get; set; } = 0;
}

public class MsgEnterBattle : MsgBase
{
    public TankInfo[] tanks { get; set; }
    public int mapId { get; set; } = 1;

    public MsgEnterBattle()
    {
        protoName = "MsgEnterBattle";
    }
}

public class MsgBattleResult : MsgBase
{
    public int winCamp { get; set; } = 0;

    public MsgBattleResult()
    {
        protoName = "MsgBattleResult";
    }
}

public class MsgLeaveBattle : MsgBase
{
    public string id { get; set; } = "";

    public MsgLeaveBattle()
    {
        protoName = "MsgLeaveBattle";
    }
}
