using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player
{
    public string id = "";
    public ClientState state;
    public float x;
    public float y;
    public float z;
    public PlayerData data;
    public float ex;
    public float ey;
    public float ez;
    public int roomId = -1;
    public int camp = 1;
    public int hp = 100;

    public Player(ClientState state)
    {
        this.state = state;
    }

    public void Send(MsgBase msgBase)
    {
        NetManager.Send(state, msgBase);
    }
}

