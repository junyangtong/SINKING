using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRow : MonoBehaviour
{
    [Header("已弃用 - 请使用PlatformGenerator")]
    [Tooltip("此脚本已被PlatformGenerator替代")]
    public bool isDeprecated = true;

    private float Speed = 0.5f;
    [Header("时间控制参数")]
    private float timeRemaining = 25;

    private void OnEnable()
    {
        if (isDeprecated) return;
        
        int nub = Random.Range(-12,-47);
        transform.position = new Vector3(nub,-6,0);
    }


    void Update()
    {
        if (isDeprecated) return;
        
        if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                MapPool.instance.ReturnPool(this.gameObject);
                timeRemaining = 25;
            }

        transform.position = new Vector3 (transform.position.x, transform.position.y + Speed * Time.deltaTime, transform.position.z);
    }
}
