using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MsgHandler
{
    public static void MsgRegister(ClientState c, MsgBase msgBase)
    {
        MsgRegister msg = msgBase as MsgRegister;
        if (DbManager.Register(msg.id, msg.pw))
        {
            DbManager.CreatPlayer(msg.id);
            msg.result = 0;
        }
        else
        {
            msg.result = 1;
        }
        NetManager.Send(c, msg);
    }

    public static void MsgLogin(ClientState c, MsgBase msgBase)
    {
        MsgLogin msg = msgBase as MsgLogin;
        if (!DbManager.CheckPassword(msg.id, msg.pw))
        {
            msg.result = 1;
            NetManager.Send(c, msg);
            return;
        }
        if (c.player != null)
        {
            msg.result = 1;
            NetManager.Send(c, msg);
            return;
        }
        //玩家已在线，再次登录就把原先的客户端踢掉
        if (PlayerManager.IsOnline(msg.id))
        {
            Player other = PlayerManager.GetPlayer(msg.id);
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 0;
            other.Send(msgKick);
            NetManager.Close(other.state);
        }
        PlayerData playerData = DbManager.GetPlayerData(msg.id);
        if (playerData == null)
        {
            msg.result = 1;
            NetManager.Send(c, msg);
            return;
        }
        Player player = new Player(c);
        player.id = msg.id;
        player.data = playerData;
        PlayerManager.AddPlayer(msg.id, player);
        c.player = player;
        msg.result = 0;
        player.Send(msg);
    }
}
