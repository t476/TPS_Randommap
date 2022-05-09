//这里也可以选用更加轻量级的Struct、scriptableObject
//自定义了一个类,和map类一样
using System.Drawing;

[System.Serializable]//在inspector窗口序列化，可视
public class Wave 
{
    public int enemyNum;//每一波敌人的总个数
    public float timeBtwSpawn;//每一波前后敌人出现的时间间隔

    //hum
    public float moveSpeed;
    public float enemtHealth;
    public float hitPower;
    public Color skinColor;

}
