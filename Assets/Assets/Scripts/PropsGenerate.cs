using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsGenerate : MonoBehaviour
{
    public GameObject DontDie;
    public GameObject Smaller;
    public GameObject Faster;
    private float timeRemaining = 3;
    int RandWidth;
    int RandHeight;
    Vector3 Randpos;
    private GameObject DontDieInstant;
    private GameObject SmallerInstant;
    private GameObject FasterInstant;
    private void Start() {

        //免死
        DontDieInstant = Instantiate(DontDie);
        DontDieInstant.SetActive(false);
        //变小
        SmallerInstant = Instantiate(Smaller);
        SmallerInstant.SetActive(false);
        //加速
        FasterInstant = Instantiate(Faster);
        FasterInstant.SetActive(false);
    }
    void Update()
    {
        //道具生成计时器
        if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                int nub = Random.Range(1,4);
                switch(nub)
                {
                    case 1:
                        dontDie();
                        break;
                    case 2:
                        smaller();
                        break;
                    case 3:
                        faster();
                        break;
                }
                timeRemaining = 3;
            }
    }
    private void dontDie()
    {
        //随机屏幕坐标位置
        RandWidth = Random.Range(0,Screen.width);
        RandHeight = Random.Range(0,Screen.height);
        Randpos = new Vector3(RandWidth,RandHeight,0);
        //屏幕坐标-->世界坐标
        Randpos = Camera.main.ScreenToWorldPoint(Randpos);

        DontDieInstant.transform.position = new Vector3(Randpos.x, Randpos.y, 0);
        DontDieInstant.SetActive(true);
        Debug.Log("免死道具");
    }
    private void smaller()
    {
        //随机屏幕坐标位置
        RandWidth = Random.Range(0,Screen.width);
        RandHeight = Random.Range(0,Screen.height);
        Randpos = new Vector3(RandWidth,RandHeight,0);
        //屏幕坐标-->世界坐标
        Randpos = Camera.main.ScreenToWorldPoint(Randpos);

        SmallerInstant.transform.position = new Vector3(Randpos.x, Randpos.y, 0);
        SmallerInstant.SetActive(true);
        Debug.Log("变小道具");
    }
    private void faster()
    {
        //随机屏幕坐标位置
        RandWidth = Random.Range(0,Screen.width);
        RandHeight = Random.Range(0,Screen.height);
        Randpos = new Vector3(RandWidth,RandHeight,0);
        //屏幕坐标-->世界坐标
        Randpos = Camera.main.ScreenToWorldPoint(Randpos);

        FasterInstant.transform.position = new Vector3(Randpos.x, Randpos.y, 0);
        FasterInstant.SetActive(true);
        Debug.Log("加速道具");
    }
}
