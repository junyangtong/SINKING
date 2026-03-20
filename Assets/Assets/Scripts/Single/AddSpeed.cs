using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSpeed : MonoBehaviour
{
    public float Speed = 0.5f;
    private float timeRemaining = 5f;//加速间隔时间

    void Update()
    {
        if(Speed >= 1.4)
        {
            timeRemaining=0;
            Debug.Log("结束加速");
        }
        if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                Speed += 0.02f;
                timeRemaining = 10f;
            }
    }
}
