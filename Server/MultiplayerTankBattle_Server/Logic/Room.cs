using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Room
{
    public int id = 0;
    public int maxPlayer = 6;
    public Dictionary<string, bool> playerIds = new Dictionary<string, bool>();
    public string ownerId = ""; //房主id

    /// <summary>
    /// 玩家初始的出生位置
    /// </summary>
    static float[,,] birthConfig = new float[2, 3, 6]
    {
        {
            { 32f, 1.85f, -368.5f, 6f,0f,-0.5f },
            { 7f, 2.8f, -368.5f, 4.7f, 0f, -6.5f},
            { 55f, 1.9f, -358.5f,5.8f,0f,1.9f},
        },
        {
            { -320f,11.7f,326f,11f,182f,-0.5f},
            {-346f,11.5f,326f,10f,175.4f,-0.5f },
            { -267f,12f,326f,14.7f,181f,0.16f},
        },
    };

    public enum Status
    {
        PREPARE = 0,
        FIGHT = 1,
    }
    public Status status = Status.PREPARE;

    private long lastJudgeTime = 0;//上次判断是否有阵营胜利的时间

    public bool AddPlayer(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("room.AddPlayer fail, player is null");
            return false;
        }
        if (playerIds.Count >= maxPlayer)
        {
            Console.WriteLine("room.AddPlayer fail, reach maxPlayer");
            return false;
        }
        if (status != Status.PREPARE)
        {
            Console.WriteLine("room.AddPlayer fail, not PREPARE");
            return false;
        }
        if (playerIds.ContainsKey(id))
        {
            Console.WriteLine("room.AddPlayer fail, already in this room");
            return false;
        }
        playerIds[id] = true;
        player.camp = SwitchCamp();
        player.roomId = this.id;
        if (ownerId == "")
        {
            ownerId = player.id;
        }
        Broadcast(ToMsg());
        return true;
    }

    /// <summary>
    /// 分配玩家的阵营
    /// </summary>
    /// <returns>分配的阵营</returns>
    public int SwitchCamp()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == 1)
            {
                ++count1;
            }
            if (player.camp == 2)
            {
                ++count2;
            }
        }
        if (count1 <= count2)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    public bool RemovePlayer(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("room.RemovePlayer fail, player is null");
            return false;
        }
        if (!playerIds.ContainsKey(id))
        {
            Console.WriteLine("room.RemovePlayer fail, not in this room");
            return false;
        }
        playerIds.Remove(id);
        player.camp = 0;
        player.roomId = -1;
        if (IsOwner(player))
        {
            ownerId = SwitchOwner();
        }
        if (status == Status.FIGHT)
        {
            ++player.data.lost;
            MsgLeaveBattle msg = new MsgLeaveBattle();
            msg.id = player.id;
            Broadcast(msg);
        }
        //没有玩家的房间需要移除掉
        if (playerIds.Count == 0)
        {
            RoomManager.RemoveRoom(this.id);
        }
        Broadcast(ToMsg());
        return true;
    }

    public bool IsOwner(Player player)
    {
        return player.id == ownerId;
    }

    /// <summary>
    /// 分配房主
    /// </summary>
    /// <returns>房主id，放回空字符串则没有房主，此时房间已经没人</returns>
    public string SwitchOwner()
    {
        foreach (string id in playerIds.Keys)
        {
            return id;
        }
        return "";
    }

    /// <summary>
    /// 向房间里所有客户端广播消息
    /// </summary>
    /// <param name="msg">消息</param>
    public void Broadcast(MsgBase msg)
    {
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            player.Send(msg);
        }
    }

    /// <summary>
    /// 将当前房间的所有玩家的信息打包成房间信息
    /// </summary>
    /// <returns>房间信息</returns>
    public MsgBase ToMsg()
    {
        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        int count = playerIds.Count;
        msg.players = new PlayerInfo[count];
        int i = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.id = player.id;
            playerInfo.camp = player.camp;
            playerInfo.win = player.data.win;
            playerInfo.lost = player.data.lost;
            playerInfo.isOwner = 0;
            if (IsOwner(player))
            {
                playerInfo.isOwner = 1;
            }
            msg.players[i] = playerInfo;
            ++i;
        }
        return msg;
    }

    public bool CanStartBattle()
    {
        if (status != Status.PREPARE)
        {
            return false;
        }
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == 1)
            {
                ++count1;
            }
            else
            {
                ++count2;
            }
        }
        if (count1 < 1 || count2 < 1)
        {
            return false;
        }
        return true;
    }

    private void SetBirthPos(Player player, int index)
    {
        int camp = player.camp;
        player.x = birthConfig[camp - 1, index, 0];
        player.y = birthConfig[camp - 1, index, 1];
        player.z = birthConfig[camp - 1, index, 2];
        player.ex = birthConfig[camp - 1, index, 3];
        player.ey = birthConfig[camp - 1, index, 4];
        player.ez = birthConfig[camp - 1, index, 5];
    }

    private void ResetPlayers()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == 1)
            {
                SetBirthPos(player, count1);
                ++count1;
            }
            else
            {
                SetBirthPos(player, count2);
                ++count2;
            }
        }

        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            player.hp = 100;
        }
    }

    /// <summary>
    /// 从玩家的信息提取坦克信息
    /// </summary>
    /// <param name="player">玩家</param>
    /// <returns>坦克信息</returns>
    public TankInfo PlayerToTankInfo(Player player)
    {
        TankInfo tankInfo = new TankInfo();
        tankInfo.camp = player.camp;
        tankInfo.id = player.id;
        tankInfo.hp = player.hp;
        tankInfo.x = player.x;
        tankInfo.y = player.y;
        tankInfo.z = player.z;
        tankInfo.ex = player.ex;
        tankInfo.ey = player.ey;
        tankInfo.ez = player.ez;
        return tankInfo;
    }

    public bool StartBattle()
    {
        if (!CanStartBattle())
        {
            return false;
        }
        status = Status.FIGHT;
        ResetPlayers();
        MsgEnterBattle msg = new MsgEnterBattle();
        msg.mapId = 1;
        msg.tanks = new TankInfo[playerIds.Count];
        int i = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            msg.tanks[i] = PlayerToTankInfo(player);
            ++i;
        }
        Broadcast(msg);
        return true;
    }

    public bool IsDie(Player player)
    {
        return player.hp <= 0;
    }

    /// <summary>
    /// 判断是否有阵营胜利，当另一方没有存活的玩家时，本阵营胜利
    /// </summary>
    /// <returns>胜利的阵营，返回0则尚未分出胜负</returns>
    public int Judgment()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (!IsDie(player))
            {
                if (player.camp == 1)
                {
                    ++count1;
                }
                if (player.camp == 2)
                {
                    ++count2;
                }
            }
        }
        if (count1 <= 0)
        {
            return 2;
        }
        else if (count2 <= 0)
        {
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// 每个10秒判断一次是否有阵营胜利，需要在外界调用
    /// </summary>
    public void Update()
    {
        if (status != Status.FIGHT)
        {
            return;
        }
        if (NetManager.GetTimeStamp() - lastJudgeTime < 10f)
        {
            return;
        }
        lastJudgeTime = NetManager.GetTimeStamp();
        int winCamp = Judgment();
        if (winCamp == 0)
        {
            return;
        }
        status = Status.PREPARE;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == winCamp)
            {
                ++player.data.win;
            }
            else
            {
                ++player.data.lost;
            }
        }
        MsgBattleResult msg = new MsgBattleResult();
        msg.winCamp = winCamp;
        Broadcast(msg);
    }
}
