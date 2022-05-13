using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public BaseTank tank;
    private GameObject skin;
    Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init()
    {
        GameObject skinRes = ResManager.LoadPrefab("bulletPrefab");
        skin = Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collObj = collision.gameObject;
        BaseTank hitTank = collObj.GetComponent<BaseTank>();
        if (hitTank == tank)
        {
            return;
        }
        if (hitTank != null)
        {
            SendMsgHit(tank, hitTank);
        }
        GameObject explode = ResManager.LoadPrefab("explosion");
        GameObject go = Instantiate(explode, transform.position, transform.rotation);
        Destroy(go, 2f);
        Destroy(gameObject);
    }

    void SendMsgHit(BaseTank tank, BaseTank hitTank)
    {
        if (hitTank == null || tank == null)
        {
            return;
        }
        if (tank.id != GameMain.id)
        {
            return;
        }
        MsgHit msg = new MsgHit();
        msg.targetId = hitTank.id;
        msg.id = tank.id;
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        NetManager.Send(msg);
    }
}
