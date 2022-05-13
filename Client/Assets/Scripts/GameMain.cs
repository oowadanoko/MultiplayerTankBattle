using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public static string id = "";

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);
        PanelManager.Init();
        BattleManager.Init();
        PanelManager.Open<LoginPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }

    void OnConnectClose(string err)
    {
        Debug.Log("�Ͽ�����");
    }

    void OnMsgKick(MsgBase msgBase)
    {
        PanelManager.Open<TipPanel>("��������");
    }
}
