using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    private Button startButton;
    private Button closeButtom;
    private Transform content;
    private GameObject playerObj;

    public override void OnInit()
    {
        base.OnInit();
        skinPath = "RoomPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        base.OnShow(para);
        startButton = skin.transform.Find("CtrlPanel/StartButton").GetComponent<Button>();
        closeButtom = skin.transform.Find("CtrlPanel/CloseButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        playerObj = skin.transform.Find("Player").gameObject;

        playerObj.SetActive(false);

        startButton.onClick.AddListener(OnStartClick);
        closeButtom.onClick.AddListener(OnCloseClick);

        NetManager.AddMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.AddMsgListener("MsgStartBattle", OnMsgStartBattle);

        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        NetManager.Send(msg);
    }

    public override void OnClose()
    {
        base.OnClose();
        NetManager.RemoveMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.RemoveMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.RemoveMsgListener("MsgStartBattle", OnMsgStartBattle);
    }

    public void OnMsgGetRoomInfo(MsgBase msgBase)
    {
        MsgGetRoomInfo msg = msgBase as MsgGetRoomInfo;
        for (int i = content.childCount - 1; i >= 0; --i)
        {
            GameObject o = content.GetChild(i).gameObject;
            Destroy(o);
        }
        if (msg.players == null)
        {
            return;
        }
        for (int i = 0; i < msg.players.Length; ++i)
        {
            GeneratePlayerInfo(msg.players[i]);
        }
    }

    public void GeneratePlayerInfo(PlayerInfo playerInfo)
    {
        GameObject o = Instantiate(playerObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;

        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text campText = trans.Find("CampText").GetComponent<Text>();
        Text scoreText = trans.Find("ScoreText").GetComponent<Text>();

        idText.text = playerInfo.id;
        if (playerInfo.camp == 1)
        {
            campText.text = "红";
        }
        else
        {
            campText.text = "蓝";
        }
        if (playerInfo.isOwner == 1)//房主在阵营后加感叹号以示标识
        {
            campText.text += "！";
        }
        scoreText.text = playerInfo.win + "胜 " + playerInfo.lost + "负";
    }

    public void OnCloseClick()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
    }

    public void OnMsgLeaveRoom(MsgBase msgBase)
    {
        MsgLeaveRoom msg = msgBase as MsgLeaveRoom;
        if (msg.result == 0)
        {
            PanelManager.Open<TipPanel>("退出房间");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("退出房间失败");
        }
    }

    public void OnStartClick()
    {
        MsgStartBattle msg = new MsgStartBattle();
        NetManager.Send(msg);
    }

    public void OnMsgStartBattle(MsgBase msgBase)
    {
        MsgStartBattle msg = msgBase as MsgStartBattle;
        if (msg.result == 0)
        {
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("开战失败！两队至少都需要一名玩家，只有队长可以开始战斗！");
        }
    }
}
