using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRowSingle : MonoBehaviour
{
    private float Speed;
    [Header("时间控制参数")]
    private float timeRemaining = 25;//持续时间

    private void OnEnable()
    {
        int nub = Random.Range(-12,-47);
        transform.position = new Vector3(nub,-6,0);
    }
        
    void Update()
    {
        Speed = GameObject.Find("AddSpeed").GetComponent<AddSpeed>().Speed;
        
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
