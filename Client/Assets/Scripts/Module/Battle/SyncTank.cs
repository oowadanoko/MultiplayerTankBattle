using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTank : BaseTank
{
    private Vector3 lastPos;//上次的位置
    private Vector3 lastRot;//上次的旋转
    private Vector3 forecastPos;//预测的位置
    private Vector3 forecastRot;//预测的旋转
    private float forecastTime;//上次预测时间

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
    /// 持续预测并设定当前坦克的位置旋转信息
    /// </summary>
    public void ForecastUpdate()
    {
        float t = (Time.time - forecastTime) / CtrlTank.syncInterval;
        t = Mathf.Clamp(t, 0f, 1f);
        Vector3 pos = transform.position;
        pos = Vector3.Lerp(pos, forecastPos, t);//从当前位置移动到预测的位置
        transform.position = pos;
        Quaternion quat = transform.rotation;
        Quaternion forecastQuat = Quaternion.Euler(forecastRot);
        quat = Quaternion.Lerp(quat, forecastQuat, t);//从当前旋转到预测的旋转
        transform.rotation = quat;
    }

    /// <summary>
    /// 同步位置
    /// </summary>
    /// <param name="msg">同步坦克消息</param>
    public void SyncPos(MsgSyncTank msg)
    {
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        //根据同步位置预测坦克下次的位置和旋转
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
    /// 同步开火
    /// </summary>
    /// <param name="msg">开火消息</param>
    public void SyncFire(MsgFire msg)
    {
        Bullet bullet = Fire();
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        //在消息指定的位置开火
        bullet.transform.position = pos;
        bullet.transform.eulerAngles = rot;
    }
}
