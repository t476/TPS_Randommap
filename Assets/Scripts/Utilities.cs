using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    //工具类重新排序后返回瓦片集合,洗牌算法：swap
    //在别的类里调用的方法
    public static T[] ShuffleCoords<T>(T[] _dataArray)
    {
        for(int i = 0; i < _dataArray.Length; i++)
        {
            int randomNum = Random.Range(i, _dataArray.Length);
            T temp = _dataArray[randomNum];
            _dataArray[randomNum] = _dataArray[i];
            _dataArray[i] = temp;


        }
        return _dataArray; 
    }
}
