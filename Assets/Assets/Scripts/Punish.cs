using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punish : MonoBehaviour
{
    private float timeRemaining = 0;

    // Update is called once per frame
    private void OnEnable() {
        timeRemaining = 5;
    }
    void Update()
    {
        //惩罚消失计时器
        if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                gameObject.SetActive(false);
                timeRemaining = 5;
            }
    }
}
