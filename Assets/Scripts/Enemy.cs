using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : LivingEntity
{
    public enum State {Idle,Chasing,Attacking};
    State currentState;

    //敌人死亡效果
    public GameObject deatheffect;
    public float attackSpeed = 1f;//做个减速弹吧
    //配合navmeshagent使用
    private NavMeshAgent navMeshAgent;
    private Transform target;//追击player
    LivingEntity targetEntity;
    //写一下敌人攻击player
    float attackDistanceThreshold = 1.5f;
    float nextAttackTime;
    float AttackTime=1f;
    float damage = 1f;
    //攻击后player变色
    Material playerSkinMaterial;
    Color originalColor;
    Material enemySkinMaterial;
    Color originalEnemyColor;
    bool hasTarget;

    bool changedSpeed;
    float yanchiTime = 0;
    float recoverTime = 0;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemySkinMaterial = GetComponent<Renderer>().material;
        originalEnemyColor = enemySkinMaterial.color;

        //代码严谨性
        currentState = State.Chasing;
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        //StartCoroutine(updatePath());
        Invoke("PlayerMaterialInvoke", 0.2f);
    }
    public override void TakenHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);//打到了

        if (damage >= health)
        {
            AudioManager.instance.PlaySound("EnemyDeath", transform.position);
            //  Destroy(Instantiate(deatheffect, hitPoint, transform.rotation) as GameObject, 2f);
            //太要命了，方向越来越离谱
            Destroy(Instantiate(deatheffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, 2f);
        }
        base.TakenHit(damage, hitPoint, hitDirection);
    }
    
    public override void ChangeSpeed()
    {
        
        changedSpeed = true;
        navMeshAgent.enabled = false;
        Debug.Log("jiansu2");
        yanchiTime =3+Time.time;
        attackSpeed = 0.5f;
        enemySkinMaterial.color = Color.blue;
       

        StartCoroutine("WaitForSecond");

    }
    IEnumerator WaitForSecond()
    {
        //3s之前
     //差不多还是好用的

        yield return new WaitForSeconds(3);
        //3s之后
 
        if (yanchiTime <= Time.time)
        {
           
            RecoverSpeed();
         }
      

    }
   
    public override void RecoverSpeed()
    {
        Debug.Log("444");
        navMeshAgent.enabled = true;
        changedSpeed = false;
        attackSpeed = 1f;
        enemySkinMaterial.color = originalEnemyColor;

    }
    public void SetCharacteristics(float moveSpeed, float hitPower, float enemyHealth)
    {
       
       //只能在Start函数和Awake函数中成功的改变nav速度数值，所以不改这个了
        if (hasTarget)
        {
            damage = hitPower;
        }
        maxHealth = enemyHealth;
        
    }
    private void PlayerMaterialInvoke()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            playerSkinMaterial = target.GetComponent<Renderer>().material;
            targetEntity = target.GetComponent<LivingEntity>();
            ////每当生成新的敌人，就要将这个player死亡后敌人的的阵亡事件处理器，订阅到事件onDeath上
            targetEntity.onDeath += OnTargetDeath;
            originalColor = playerSkinMaterial.color;
            Debug.Log(originalColor);
        }
    }
    
    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }
    bool state = false;
    /*
    public void SetCharacteristics( Color skinColour)
    {
      
      //  deathEffect.startColor = new Color(skinColour.r, skinColour.g, skinColour.b, 1);
        skinMaterial = GetComponent<Renderer>().sharedmaterial;
        skinMaterial.color = skinColour;
        originalColour = skinMaterial.color;
    }*/
    void Update()
    {
        
        //if (!target)不能这么写的啦，target会更新的啦
        if(GameObject.FindGameObjectWithTag("Player") != null&&state==false)
        {
            state = true;
            //target = GameObject.FindGameObjectWithTag("Player").transform;
            StartCoroutine(updatePath());
            
        }

        if (hasTarget)
        {
            //用vector3.distance有点费，只需要在平面判断距离
            float squareDstToTarget = (target.position - transform.position).sqrMagnitude;
            if (squareDstToTarget < Mathf.Pow(attackDistanceThreshold, 2))
            {
                if (Time.time >= nextAttackTime)
                {
                    //attack
                    AudioManager.instance.PlaySound("EnemyAttack", transform.position);
                    StartCoroutine(Attack());
                    nextAttackTime = Time.time + AttackTime;
                }
            }
        }
        //这里不在update有点难实现啊，在的话有点浪费了，而且这么写是错的,显然
        //还是看看正经用协程这里怎么写改一改吧，困
        /*
        recoverTime = Time.time + yanchiTime / 1000;
        if (changedSpeed&&yanchiTime<=0f)
        {
            RecoverSpeed();
            recoverTime = Time.time + yanchiTime / 1000;
        }*/
        //锲而不舍类敌人
        // navMeshAgent.SetDestination(target.position);
        //摸鱼类敌人

    }
    IEnumerator Attack()
    {
        currentState = State.Attacking;
        navMeshAgent.enabled = false;
        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position-(dirToTarget*0.3f);
        playerSkinMaterial.color = Color.red;

        bool hasAppliedDamage = false;
        //lerp
        float percent = 0;
        //妙啊。
        while (percent <= 1)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakenDamage(damage);
                targetEntity.GetComponentInChildren<HealthBar>().hp -= damage;
                targetEntity.GetComponentInChildren<HealthBar>().UpdateHp();//在这里决定玩家受伤和死亡，我想给玩家死亡也加个效果
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-percent * percent + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            yield return null;//保證每次循环遍历都从上一次停止的地方开始执行
        }
        playerSkinMaterial.color = originalColor;
        navMeshAgent.enabled = true;
        currentState = State.Chasing;

    }

    IEnumerator updatePath()//迭代器（计数器）
    {
        float updateRate = 3.0f;
        //是不是这里一直while啊，这里一看就是。。。应该改成if吧，我真是宇宙无敌傻逼了
        //但是教程用了while也运行了，我不能理解，哦哦，人家吧yieldreturn放在while里，当然可以啦
        //而且人家的调用在start里
        while (target != null)
        {
            if (currentState == State.Chasing)
            {
                Vector3 preTargetPos = new Vector3(target.position.x, 0, target.position.z);
                navMeshAgent.SetDestination(target.position);
            }
            yield return new WaitForSeconds(updateRate);
            //这样就对了，这是为什么呢？
            //看来我对上面这个语句掌握的还是不到位啊
        }
        //有些许不理解在里面，但写都写了，先这样凑合一下
        //这明明只调用一次的东西，被我调用这么多次。。。
        //yield return new WaitForSeconds(updateRate);//过三秒再去遍历上边几行

    }
}
