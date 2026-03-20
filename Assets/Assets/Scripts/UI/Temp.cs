using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
    public int TotalTime = 180;//计时器总时间
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

}
