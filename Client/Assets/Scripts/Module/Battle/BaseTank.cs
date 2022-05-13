using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTank : MonoBehaviour
{
    private GameObject skin;
    public float steer = 20;
    public float speed = 8f;

    protected Rigidbody rigidBody;

    public float turretSpeed = 30f;
    public Transform turret;
    public Transform gun;
    public Transform firePoint;

    public float fireCd = 0.5f;
    public float lastFireTime = 0;

    public float hp = 100;
    public string id = "";
    public int camp = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void Init(string skinPath)
    {
        GameObject skinRes = ResManager.LoadPrefab(skinPath);
        skin = Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;

        rigidBody = gameObject.AddComponent<Rigidbody>();
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, 2.5f, 1.47f);
        boxCollider.size = new Vector3(7, 5, 12);

        turret = skin.transform.Find("Turret");
        gun = turret.transform.Find("Gun");
        firePoint = gun.transform.Find("FirePoint");
    }

    // Update is called once per frame
    public void Update()
    {
        
    }

    public Bullet Fire()
    {
        if (IsDie())
        {
            return null;
        }
        GameObject bulletObj = new GameObject("bullet");
        Destroy(bulletObj, 30f);
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Init();
        bullet.tank = this;
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        GameObject go = Instantiate(ResManager.LoadPrefab("smoke"), firePoint, false);
        go.transform.localPosition = Vector3.zero;
        lastFireTime = Time.time;
        return bullet;
    }

    public bool IsDie()
    {
        return hp <= 0;
    }

    public void Attacked(int att)
    {
        if (IsDie())
        {
            return;
        }
        hp -= att;
        if (IsDie())
        {
            GameObject obj = ResManager.LoadPrefab("fire");
            GameObject fire = Instantiate(obj, transform.position, transform.rotation);
            fire.transform.SetParent(transform);
        }
    }
}
