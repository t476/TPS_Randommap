using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{

   public enum FireMode { Auto, Burst, Single };
    //单射有冷却时间
   public FireMode fireMode;

    public Transform[] projectileSpawn;//不同的炮口
    public Projectile projectile;
    float nextShotTime;
    public float msBetweenShots = 100;//冷却时间0.1s，这些是不同的可以更改的
    public float muzzleVelocity = 35;
    //public int burstCount=3;//
    //还有一个关键的是攻击力，这个放在projectile里区分
  

   
    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootAudio;
   
   //枪口光
    MuzzleFlash muzzleflash;
  


    void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
     
    
    }
    /*
    void LateUpdate()在所有update发生后调用，重装枪械
    {
        // animate recoil
        //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        //recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }
    */
    public void Shoot()
    {

        if (Time.time > nextShotTime)//计时器限制发射频率
        {

            for (int i = 0; i < projectileSpawn.Length; i++)
            {

                nextShotTime = Time.time + (msBetweenShots / 1000);

                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                //PROJECTIE脚本里 给他设置速度
                newProjectile.SetSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);

            muzzleflash.Activate();


            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

  
    public void OnTriggerHold()
    {
        Shoot();
        
    }

    public void OnTriggerRelease()
    {
        
    }
}
