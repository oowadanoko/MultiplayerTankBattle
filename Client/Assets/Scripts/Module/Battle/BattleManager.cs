using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
    public static Dictionary<string, BaseTank> tanks = new Dictionary<string, BaseTank>();
    
    public static void Init()
    {
        NetManager.AddMsgListener("MsgEnterBattle", OnMsgEnterBattle);
        NetManager.AddMsgListener("MsgBattleResult", OnMsgBattleResult);
        NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);

        NetManager.AddMsgListener("MsgSyncTank", OnMsgSyncTank);
        NetManager.AddMsgListener("MsgFire", OnMsgFire);
        NetManager.AddMsgListener("MsgHit", OnMsgHit);
    }

    public static void AddTank(string id, BaseTank tank)
    {
        tanks[id] = tank;
    }

    public static void RemoveTank(string id)
    {
        tanks.Remove(id);
    }

    public static BaseTank GetTank(string id)
    {
        if (tanks.ContainsKey(id))
        {
            return tanks[id];
        }
        return null;
    }

    public static BaseTank GetCtrlTank()
    {
        return GetTank(GameMain.id);
    }

    public static void Reset()
    {
        foreach (BaseTank tank in tanks.Values)
        {
            MonoBehaviour.Destroy(tank.gameObject);
        }
        tanks.Clear();
    }

    public static void OnMsgEnterBattle(MsgBase msgBase)
    {
        MsgEnterBattle msg = msgBase as MsgEnterBattle;
        EnterBattle(msg);
    }

    public static void EnterBattle(MsgEnterBattle msg)
    {
        BattleManager.Reset();
        PanelManager.Close("RoomPanel");
        PanelManager.Close("ResultPanel");
        for (int i = 0; i < msg.tanks.Length; ++i)
        {
            GenerateTank(msg.tanks[i]);
        }
    }

    public static void GenerateTank(TankInfo tankInfo)
    {
        string objName = "Tank_" + tankInfo.id;
        GameObject tankObj = new GameObject(objName);
        BaseTank tank = null;
        if (tankInfo.id == GameMain.id)
        {
            tank = tankObj.AddComponent<CtrlTank>();
        }
        else
        {
            tank = tankObj.AddComponent<SyncTank>();
        }
        //当前客户端的坦克添加相机跟随
        if (tankInfo.id == GameMain.id)
        {
            CameraFollow cf = tankObj.AddComponent<CameraFollow>();
        }
        tank.camp = tankInfo.camp;
        tank.id = tankInfo.id;
        tank.hp = tankInfo.hp;
        Vector3 pos = new Vector3(tankInfo.x, tankInfo.y, tankInfo.z);
        Vector3 rot = new Vector3(tankInfo.ex, tankInfo.ey, tankInfo.ez);
        tank.transform.position = pos;
        tank.transform.eulerAngles = rot;
        if (tankInfo.camp == 1)
        {
            tank.Init("tankPrefab");
        }
        else
        {
            tank.Init("tankPrefab2");
        }
        AddTank(tankInfo.id, tank);
    }

    public static void OnMsgBattleResult(MsgBase msgBase)
    {
        MsgBattleResult msg = msgBase as MsgBattleResult;
        bool isWin = false;
        BaseTank tank = GetCtrlTank();
        if (tank != null && tank.camp == msg.winCamp)
        {
            isWin = true;
        }
        PanelManager.Open<ResultPanel>(isWin);
    }

    public static void OnMsgLeaveBattle(MsgBase msgBase)
    {
        MsgLeaveBattle msg = msgBase as MsgLeaveBattle;
        BaseTank tank = GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        RemoveTank(msg.id);
        MonoBehaviour.Destroy(tank.gameObject);
    }

    public static void OnMsgSyncTank(MsgBase msgBase)
    {
        MsgSyncTank msg = msgBase as MsgSyncTank;
        if (msg.id == GameMain.id)
        {
            return;
        }
        SyncTank tank = GetTank(msg.id) as SyncTank;
        if (tank == null)
        {
            return;
        }
        tank.SyncPos(msg);
    }

    public static void OnMsgFire(MsgBase msgBase)
    {
        MsgFire msg = msgBase as MsgFire;
        if (msg.id == GameMain.id)
        {
            return;
        }
        SyncTank tank = GetTank(msg.id) as SyncTank;
        if (tank == null)
        {
            return;
        }
        tank.SyncFire(msg);
    }

    public static void OnMsgHit(MsgBase msgBase)
    {
        MsgHit msg = msgBase as MsgHit;
        BaseTank tank = GetTank(msg.targetId);
        if (tank == null)
        {
            return;
        }
        tank.Attacked(msg.damage);
    }
}
