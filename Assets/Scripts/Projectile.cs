using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Gun equippedGun;
    [SerializeField] private float shootSpeed;
    public float damage = 1.0f;
    [SerializeField] private float lifetime;
    public LayerMask collisionMask;
    public Color trailColour;
     public float x, y, z;
    //public Transform originTransform;

    //改用Physics.OverlapSphere做一下
    float skinWidth = .1f;

    void Start()
    {
        equippedGun = FindObjectOfType<Gun>();//
        Destroy(gameObject, lifetime);
        x = transform.position.x;//这些是不会随时间而动的
        y = transform.position.y;
        z = transform.position.z;

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
        
            OnHitObject(initialCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColour);
    }
    
    public void SetSpeed(float newSpeed)
    {
        shootSpeed = newSpeed;
    }

    void Update()
    {
        float moveDistance = shootSpeed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(-transform.forward * moveDistance);
       // transform.Translate(Vector3.forward * moveDistance);
    }


    void CheckCollisions(float moveDistance)
    {
       // Ray ray = new Ray(transform.position, transform.forward);
        Ray ray = new Ray(new Vector3(x, y, z), transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        
        if (damageableObject != null)
        {
            //不对啊这个没转换成自身坐标系啊对于Transform.forward来说,它代表当前物体的物体坐标系的z轴在世界坐标系上的指向
            //这太奇怪了
         
            
            
            damageableObject.TakenHit(damage, hitPoint, new Vector3(transform.up.x,-transform.up.y,transform.up.z));
            c.GetComponentInChildren<HealthBar>().hp -= damage;
            c.GetComponentInChildren<HealthBar>().UpdateHp();
            Debug.Log(transform.up);
            if (equippedGun.fireMode == Gun.FireMode.Single)
            {
                Debug.Log("jiansu ");
                damageableObject.ChangeSpeed();
                //减速三秒
            }

        }
        GameObject.Destroy(gameObject);
    }
}

    /*private void Start()
    {
        Destroy(gameObject, lifetime);
        x = transform.position.x;//这些是不会随时间而动的
        y = transform.position.y;
        z = transform.position.z;
        //originTransform = gameObject.transform;这个等于直接拖的transform组件，当然会也随时间变化而变化


       // Debug.Log(gameObject.transform.position);//这里的transform.position是对的

    }
    private void Update()
    {    //为何我的子弹无尽下落,他又没有rb

       
        transform.Translate(-transform.forward * shootSpeed * Time.deltaTime);//这是一个移动
        //transform.Translate(Vector3.up* shootSpeed * Time.deltaTime);
        //transform.Translate(Vector3.forward* shootSpeed * Time.deltaTime);//他不行
        
        //会不会因为transform一直在动啊


        CheckCollision();
    }

    private void CheckCollision()
    {
        //Debug.Log("111");这个方向不一定对啊，毕竟我的这个子弹她。第21行的发射方向就有点问题
       //而且，要碰上，这得一般高啊虽然确实差不多高了。。。。
        Ray ray = new Ray(new Vector3(x, y, z), transform.up);
        //Debug.DrawRay(new Vector3(x,y,z), transform.up*100,Color.red,10,false);//咋没用啊
        //Debug.Log("@222");
        //声明一个raycasthit结构体类型：hitInfo
        RaycastHit hitInfo;
        
        //意思是physic.Raycast方法返回布尔值，还返回了一饿out修饰的raycasthit类型的数据，包含检测得到的点的详细数据
        //最后一个参数是询问是否要忽略射线方向collider组件中的isTriggr，这里选择不忽略
        if (Physics.Raycast(ray, out hitInfo, shootSpeed * Time.deltaTime, collisionMask, QueryTriggerInteraction.Collide))
            //;找到报空原因了。类目
        {
            
            //击中敌人了，敌人受伤
            HitEnemy(hitInfo);
        }
    }

   private void HitEnemy(RaycastHit _hitInfo)
    {
        //获取敌人的IDamageable接口中的TakenDamage方法来对敌人造成伤害
       
        IDamageable damageable = _hitInfo.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            
            damageable.TakenDamage(damage);
            //Destroy(gameObject);注意销毁位置
        }
        Destroy(gameObject);
    }*/

