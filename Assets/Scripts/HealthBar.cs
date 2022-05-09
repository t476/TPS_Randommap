using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image hpImage;
    public Image hpEffectImage;

    [HideInInspector] public float hp;
    [SerializeField] private float maxHp;
    [SerializeField] private float hurtSpeed = 0.005f;
    LivingEntity livingEntityObject;

    private void Awake()
    {
        
        livingEntityObject = GetComponentInParent<LivingEntity>();
        maxHp = livingEntityObject.maxHealth;
        hp= livingEntityObject.health;
        Debug.Log(maxHp);
    }
    private void Start()
    {
        //hp = maxHp;
    }
   private void Update()
    {

        transform.rotation = Camera.main.transform.rotation;
    }
    //选择用协程来减少性能消耗
    public void UpdateHp()
    {
        StartCoroutine(UpdateHpCo());
    }

    IEnumerator UpdateHpCo()
    {
        Debug.Log(hp);
        hp = livingEntityObject.health;
        hpImage.fillAmount = hp / maxHp;
        while (hpEffectImage.fillAmount >= hpImage.fillAmount)
        {
            hpEffectImage.fillAmount -= hurtSpeed;
            yield return new WaitForSeconds(0.005f);
           
        }
        if (hpEffectImage.fillAmount < hpImage.fillAmount)
        {
            hpEffectImage.fillAmount = hpImage.fillAmount;
        }
    }

}
