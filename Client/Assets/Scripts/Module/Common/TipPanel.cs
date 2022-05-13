using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    private Text text;
    private Button okBtn;

    public override void OnInit()
    {
        base.OnInit();
        skinPath = "TipPanel";
        layer = PanelManager.Layer.Tip;
    }

    public override void OnShow(params object[] para)
    {
        base.OnShow(para);
        text = skin.transform.Find("Text").GetComponent<Text>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();

        okBtn.onClick.AddListener(OnOkClick);
        if (para.Length == 1)
        {
            text.text = para[0].ToString();
        }
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public void OnOkClick()
    {
        Close();
    }
}
