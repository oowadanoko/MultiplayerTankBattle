using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgSyncTank : MsgBase
{
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float ex = 0f;
    public float ey = 0f;
    public float ez = 0f;
    public float turretY = 0f;
    public string id = "";

    public MsgSyncTank()
    {
        protoName = "MsgSyncTank";
    }
}

public class MsgFire : MsgBase
{
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float ex = 0f;
    public float ey = 0f;
    public float ez = 0f;
    public string id = "";

    public MsgFire()
    {
        protoName = "MsgFire";
    }
}

public class MsgHit : MsgBase
{
    public string targetId = "";
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public string id = "";
    public int hp = 0;
    public int damage = 0;

    public MsgHit()
    {
        protoName = "MsgHit";
    }
}