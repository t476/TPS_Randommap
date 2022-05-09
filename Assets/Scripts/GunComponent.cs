using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunComponent : MonoBehaviour
{//分开，这个专做枪的切换
    //public Transform firePoint;
    //public GameObject projectilePrefab;
    //[SerializeField] private float fireRate = 0.1f;
    public Transform weaponHold;
    public Gun[] allGuns;
    int gunIndex=0;
    int gunAmount;
    public Gun gun;
    public AudioClip switchAudio;

    // private float timer;
    private void Start()
    {
        gun = FindObjectOfType<Gun>();
        gunAmount = allGuns.Length;
    }
    public void OnTriggerHold()
    {
       
            gun.OnTriggerHold();
        
    }

    public void OnTriggerRelease()
    {
      
            gun.OnTriggerRelease();
        
    }
    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (gun != null)
            {
                Destroy(gun.gameObject);
            }
            gunIndex++;
            gunIndex %= gunAmount;
            AudioManager.instance.PlaySound(switchAudio, transform.position);
            if (gun != null)
            {
                Destroy(gun.gameObject);
            }
            
            gun = Instantiate(allGuns[gunIndex],weaponHold.position,weaponHold.rotation) as Gun;
            gun.transform.parent = weaponHold;

            gun.transform.localScale = allGuns[gunIndex].transform.localScale;
        }
    }
    /*换成3种子弹
    //计时器限制发射频率
    public void Shot()
    {
        timer += Time.deltaTime;
        if (timer > fireRate)
        {
            timer = 0;
            GameObject spawnProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
           // gun.Shoot();
           
        }
    }
    */
}
