using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunishController : MonoBehaviour
{
    
    private float timeRemaining = 10;
    public GameObject punish1;
    public GameObject punish2;
    public GameObject MapCol;
    public GameObject MainCam;
    public float shakeTime = 2f;//震动时间
    public float shakeAmount = 0.2f;//振幅
    private bool isShake;
    Vector3 originalPos;
    Vector3 MaporiginalPos;

    private int screenH;
    private int screenW;
    private GameObject punish1Effect;
    private GameObject punish2Effect;
    [Header("声音部分")]
    public GameObject shakeAudio;
    private void Start() {
        screenH = Screen.height; 
        screenW = Screen.width;
        //惩罚1
        punish1Effect = Instantiate(punish1);
        punish1Effect.SetActive(false);
        //惩罚2
        punish2Effect = Instantiate(punish2);
        punish2Effect.SetActive(false);
        //惩罚3
        isShake = false;

        shakeAudio.SetActive(false);
    }
    void Update() 
    {
        originalPos = MainCam.transform.position;
        MaporiginalPos = MapCol.transform.position;
        if (isShake==true && shakeTime >0)//抖动
        {
            shakeAudio.SetActive(true);
            MainCam.transform.position = new Vector3(Random.Range(-1,1) * shakeAmount,originalPos.y,originalPos.z);
            MapCol.transform.position = new Vector3(Random.Range(-1,1) * shakeAmount,Random.Range(-1,1) * shakeAmount,MaporiginalPos.z);
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 2f;
            isShake=false;

            shakeAudio.SetActive(false);
        }

        //惩罚时间计时器
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
                        Punish1();
                        break;
                    case 2:
                        Punish2();
                        break;
                    case 3:
                        Punish3();
                        break;
                    default:
                        Debug.Log("惩罚不是123中任何一个？！");
                        break;
                }
                timeRemaining = 10;
            }
    }
    private void Punish1()
    {
        //随机屏幕坐标位置
        int RandWidth = Random.Range(0,screenW);
        int RandHeight = Random.Range(0,screenH);

        Vector3 Randpos = new Vector3(RandWidth,RandHeight,0);
        //屏幕坐标-->世界坐标
        Randpos = Camera.main.ScreenToWorldPoint(Randpos);
        punish1Effect.transform.position = new Vector3(Randpos.x, Randpos.y, 0);
        punish1Effect.SetActive(true);
        //Instantiate(punish1,Randpos,Quaternion.identity);
        Debug.Log("惩罚1");
    }
    private void Punish2()
    {
        int RandWidth = Random.Range(0,screenW);
        Vector3 Randpos = new Vector3(RandWidth,0,0);
        //屏幕坐标-->世界坐标
        Randpos = Camera.main.ScreenToWorldPoint(Randpos);
        punish2Effect.transform.position = new Vector3(Randpos.x, 1, 0);
        punish2Effect.SetActive(true);
        Debug.Log("惩罚2");
    }
    private void Punish3()
    {
        isShake=true;
        Debug.Log("惩罚3");
    }
        
}
