using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : BasePanel
{
    private Image winImage;
    private Image lostImage;
    private Button okBtn;

    public override void OnInit()
    {
        base.OnInit();
        skinPath = "ResultPanel";
        layer = PanelManager.Layer.Tip;
    }

    public override void OnShow(params object[] para)
    {
        base.OnShow(para);
        winImage = skin.transform.Find("WinImage").GetComponent<Image>();
        lostImage = skin.transform.Find("LostImage").GetComponent<Image>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();
        okBtn.onClick.AddListener(OnOkClick);
        if (para.Length == 1)
        {
            bool isWin = (bool)para[0];
            if (isWin)
            {
                winImage.gameObject.SetActive(true);
                lostImage.gameObject.SetActive(false);
            }
            else
            {
                winImage.gameObject.SetActive(false);
                lostImage.gameObject.SetActive(true);
            }
        }
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public void OnOkClick()
    {
        PanelManager.Open<RoomPanel>();
        Close();
    }
}
