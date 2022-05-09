using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//这是playerController和enemy的基类·（父类）
//C#不支持多继承，只能继承一个父类，我们可以使用多个接口/一个父类配合多个接口来实现相同的效果
public class LivingEntity : MonoBehaviour,IDamageable
{
    public float maxHealth;
    public float health;//他的级别是外界不能访问但是允许继承自livingentity的子类可以访问
    private bool isDead;

    //阵亡：一个事件
    //（1）事件发布者定义event以及事件相关信息  （2）事件侦听者订阅event   （3）事件发布者触发event，自动调用订阅者的事件处理方法。
    //事件的简略声明格式：private/public +event key +Delegate(委托类型)+eventName（一般会使用on做前缀）
    //事件是不会主动发生的，一定是事件的拥有者的某些内部逻辑触发的
    public event Action onDeath;
    //子类父类的start方法如何同时执行？
    //通过关键词virtual明确告知，这个start方法我们允许在子类中复写/重写
    //也可以使用base.start方法在父类的基础上添加额外的部分
    protected virtual void Start()
    {
        health = maxHealth;
    }

    //触发事件的逻辑
    [ContextMenu("Self Destruct")]//创建一个上下文菜单用来右键自毁，cool
    public virtual void Die()
    {
        isDead = true;
        Destroy(gameObject);
        if (onDeath != null)
            onDeath();
        //onDeath事件：如果是player，gameoverUI，取消敌人对player的追踪
        //如果是敌人，判断场上剩余敌人数量，安排下一波
    }

    //敌人收到攻击,粒子效果,可在emeny里override
    public virtual void TakenHit(float damage,Vector3 hitPoint,Vector3 hitDirection)
    {
        TakenDamage(damage);

    }

    public virtual void TakenDamage(float _damageAmount)
    {
        health -= _damageAmount;
        if (health <= 0 && isDead == false)
        {
            Die();
        }
    }
    public virtual void ChangeSpeed()
    {

    }
    public virtual void RecoverSpeed()
    {

    }
}
