using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTank : BaseTank
{
    private Vector3 lastPos;//�ϴε�λ��
    private Vector3 lastRot;//�ϴε���ת
    private Vector3 forecastPos;//Ԥ���λ��
    private Vector3 forecastRot;//Ԥ�����ת
    private float forecastTime;//�ϴ�Ԥ��ʱ��

    new void Update()
    {
        base.Update();
        ForecastUpdate();
    }

    public override void Init(string skinPath)
    {
        base.Init(skinPath);
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        rigidBody.useGravity = false;
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        forecastPos = transform.position;
        forecastRot = transform.eulerAngles;
        forecastTime = Time.time;
    }

    /// <summary>
    /// ����Ԥ�Ⲣ�趨��ǰ̹�˵�λ����ת��Ϣ
    /// </summary>
    public void ForecastUpdate()
    {
        float t = (Time.time - forecastTime) / CtrlTank.syncInterval;
        t = Mathf.Clamp(t, 0f, 1f);
        Vector3 pos = transform.position;
        pos = Vector3.Lerp(pos, forecastPos, t);//�ӵ�ǰλ���ƶ���Ԥ���λ��
        transform.position = pos;
        Quaternion quat = transform.rotation;
        Quaternion forecastQuat = Quaternion.Euler(forecastRot);
        quat = Quaternion.Lerp(quat, forecastQuat, t);//�ӵ�ǰ��ת��Ԥ�����ת
        transform.rotation = quat;
    }

    /// <summary>
    /// ͬ��λ��
    /// </summary>
    /// <param name="msg">ͬ��̹����Ϣ</param>
    public void SyncPos(MsgSyncTank msg)
    {
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        //����ͬ��λ��Ԥ��̹���´ε�λ�ú���ת
        forecastPos = pos + 2 * (pos - lastPos);
        forecastRot = rot + 2 * (rot - lastRot);
        lastPos = pos;
        lastRot = rot;
        forecastTime = Time.time;
        Vector3 le = turret.localEulerAngles;
        le.y = msg.turretY;
        turret.localEulerAngles = le;
    }

    /// <summary>
    /// ͬ������
    /// </summary>
    /// <param name="msg">������Ϣ</param>
    public void SyncFire(MsgFire msg)
    {
        Bullet bullet = Fire();
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        //����Ϣָ����λ�ÿ���
        bullet.transform.position = pos;
        bullet.transform.eulerAngles = rot;
    }
}
