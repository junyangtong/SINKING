using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerSingle : MonoBehaviour
{
    private int TotalTime=0;//总时间
    public Text TimeText;//游戏中显示时间
    public Text FinalTime;//结算界面显示时间
    public Text BestTime;//最好成绩
    private int mumite;//分
    private int second;//秒
    public int Player1DiedNum;
    private void Awake() {

    }
    private void Update() 
    {
        Player1DiedNum = GameObject.Find("Player1").GetComponent<PlayControllerSingle>().Player1DiedNum;
        if(Player1DiedNum == 2)
        {
            
            StopCoroutine("startTime");
            FinalTime.text = "生存时间:" + TimeText.text;
            ReplaceBestScore(TotalTime);
        }
    }

    void Start()
    {
        StartCoroutine("startTime");
    }
    public IEnumerator startTime()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (TotalTime >= 0)
        {
            yield return waitForSeconds;
            TotalTime++;
            TimeText.text = "Time:" + TotalTime;
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
            
        }
    }
    public void ReplaceBestScore(int nowScore)
    {
        string temp;
        //这是设定一个存储float类型的数据的位置
        int histotyHighScore = PlayerPrefs.GetInt("historyHighScore", 0);
        if (nowScore>histotyHighScore)
        {
            //将上面的位置的数据进行替换
            PlayerPrefs.SetInt("historyHighScore",nowScore);
        }
        //换算成分秒
        mumite = histotyHighScore / 60; //输出显示分
        second = histotyHighScore % 60; //输出显示秒
        //如果秒大于10的时候，就输出格式为 00：00
            if (second >= 10)
            {
                temp = mumite + ":" + second;
            }
            //如果秒小于10的时候，就输出格式为 00：00
            else
                temp = "0" + mumite + ":0" + second;

        BestTime.text = "历史最佳成绩:" + temp;
    }
}
