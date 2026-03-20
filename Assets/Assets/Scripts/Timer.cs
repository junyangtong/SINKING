using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private int TotalTime = 180;//总时间
    public Text TimeText;//在UI里显示时间
    public GameObject GameOverUI;
    public GameObject StopGameUI;
    public GameObject P1WinUI;
    public GameObject P2WinUI;
    public GameObject NoWinUI;
    private int mumite;//分
    private int second;//秒
    private bool isStop = false;
    public int Player1DiedNum;
    public int Player2DiedNum;

    private void Update() 
    {
        

        Player1DiedNum = GameObject.Find("Player1").GetComponent<PlayController>().Player1DiedNum;
        Player2DiedNum = GameObject.Find("Player2").GetComponent<PlayController>().Player2DiedNum;
        //计时结束游戏暂停
        if (isStop == true)
        {
            StopCoroutine("startTime");
            Time.timeScale = 0;
            GameOverUI.SetActive(true);
            StopGameUI.SetActive(false);
            TimeText.text = " ";
            if (Player1DiedNum>Player2DiedNum)
            {
                P2WinUI.SetActive(true);
            }
            else if (Player1DiedNum == Player2DiedNum)
            {
                NoWinUI.SetActive(true);
            }else
            {
                P1WinUI.SetActive(true);
            }
            isStop = false;

        }

    }

    void Start()
    {
        StartCoroutine("startTime");
        GameOverUI.SetActive(false);
    }
    public IEnumerator startTime()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (TotalTime >= 0)
        {
            yield return waitForSeconds;
            TotalTime--;
            TimeText.text = "Time:" + TotalTime;

            if (TotalTime <= 0)
            {                
                Debug.Log("战斗结束！");
            }
            mumite = TotalTime / 60; //输出显示分
            second = TotalTime % 60; //输出显示秒
            string length = mumite.ToString();
            //如果秒大于10的时候，就输出格式为 00：00
            if (second >= 10)
            {
                TimeText.text = mumite + ":" + second;
            }
            //如果秒小于10的时候，就输出格式为 00：00
            else
                TimeText.text = "0" + mumite + ":0" + second;
            if (second < 0)
            {
                Debug.Log("计时结束！");
                isStop = true;
            }
            
        }
    }
}
