using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgSyncTank : MsgBase
{
    public float x { get; set; } = 0f;
    public float y { get; set; } = 0f;
    public float z { get; set; } = 0f;
    public float ex { get; set; } = 0f;
    public float ey { get; set; } = 0f;
    public float ez { get; set; } = 0f;
    public float turretY { get; set; } = 0f;
    public string id { get; set; } = "";

    public MsgSyncTank()
    {
        protoName = "MsgSyncTank";
    }
}

public class MsgFire : MsgBase
{
    public float x { get; set; } = 0f;
    public float y { get; set; } = 0f;
    public float z { get; set; } = 0f;
    public float ex { get; set; } = 0f;
    public float ey { get; set; } = 0f;
    public float ez { get; set; } = 0f;
    public string id { get; set; } = "";

    public MsgFire()
    {
        protoName = "MsgFire";
    }
}

public class MsgHit : MsgBase
{
    public string targetId { get; set; } = "";
    public float x { get; set; } = 0f;
    public float y { get; set; } = 0f;
    public float z { get; set; } = 0f;
    public string id { get; set; } = "";
    public int hp { get; set; } = 0;
    public int damage { get; set; } = 0;

    public MsgHit()
    {
        protoName = "MsgHit";
    }
}
