using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControlScrip : MonoBehaviour
{
    public void Time1()
	{
		GameObject.Find("Temp").GetComponent<Temp>().TotalTime = 180;
	}
    public void Time2()
	{
		GameObject.Find("Temp").GetComponent<Temp>().TotalTime = 360;
	}
    public void Time3()
	{
		GameObject.Find("Temp").GetComponent<Temp>().TotalTime = 1200;
	}
}
