using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
    private Text idText;
    private Text scoreText;
    private Button createButton;
    private Button refreshButton;
    private Transform content;
    private GameObject roomObj;

    public override void OnInit()
    {
        base.OnInit();
        skinPath = "RoomListPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        base.OnShow(para);
        idText = skin.transform.Find("InfoPanel/IdText").GetComponent<Text>();
        scoreText = skin.transform.Find("InfoPanel/ScoreText").GetComponent<Text>();
        createButton = skin.transform.Find("CtrlPanel/CreateButton").GetComponent<Button>();
        refreshButton = skin.transform.Find("CtrlPanel/RefreshButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        roomObj = skin.transform.Find("Room").gameObject;

        createButton.onClick.AddListener(OnCreateClick);
        refreshButton.onClick.AddListener(OnRefreshClick);

        roomObj.SetActive(false);
        idText.text = GameMain.id;

        NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);

        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);
    }

    public override void OnClose()
    {
        base.OnClose();
        NetManager.RemoveMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
    }

    public void OnMsgGetAchieve(MsgBase msgBase)
    {
        MsgGetAchieve msg = msgBase as MsgGetAchieve;
        scoreText.text = msg.win + "胜 " + msg.lost + "负";
    }

    public void OnMsgGetRoomList(MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;
        for (int i = content.childCount - 1; i >= 0; --i)
        {
            GameObject o = content.GetChild(i).gameObject;
            Destroy(o);
        }
        if (msg.rooms == null)
        {
            return;
        }
        for (int i = 0; i < msg.rooms.Length; ++i)
        {
            GenerateRoom(msg.rooms[i]);
        }
    }

    public void GenerateRoom(RoomInfo roomInfo)
    {
        GameObject o = Instantiate(roomObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;

        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Button btn = trans.Find("JoinButton").GetComponent<Button>();

        idText.text = roomInfo.id.ToString();
        countText.text = roomInfo.count.ToString();
        if (roomInfo.status == 0)
        {
            statusText.text = "准备中";
        }
        else
        {
            statusText.text = "战斗中";
        }

        btn.name = idText.text;
        btn.onClick.AddListener(() => OnJoinClick(btn.name));
    }

    public void OnJoinClick(string idString)
    {
        MsgEnterRoom msg = new MsgEnterRoom();
        msg.id = int.Parse(idString);
        NetManager.Send(msg);
    }

    public void OnMsgEnterRoom(MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        if (msg.result == 0)
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("进入房间失败");
        }
    }

    public void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }

    public void OnMsgCreateRoom(MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        if (msg.result == 0)
        {
            PanelManager.Open<TipPanel>("创建成功");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("创建房间失败");
        }
    }

    public void OnRefreshClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }
}
