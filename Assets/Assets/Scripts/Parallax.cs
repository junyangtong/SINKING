using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform Cam;
    public float moveRate;
    private float startPointX;
    private float camStartX; 
    private float startPointY;
    private float camStartY;

    public bool LockY;
    public bool isPortraitMode = false;
    public bool LockX;

    void Start()
    {
        startPointX = transform.position.x;
        camStartX = Cam.transform.position.x;
        startPointY = transform.position.y;
        camStartY = Cam.transform.position.y;
    }

    void Update()
    {
        if (isPortraitMode)
        {
            if (LockX)
            {
                transform.position = new Vector3(transform.position.x, startPointY + (Cam.position.y - camStartY) * moveRate, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(startPointX + (Cam.position.x - camStartX) * moveRate, startPointY + (Cam.position.y - camStartY) * moveRate, transform.position.z);
            }
        }
        else
        {
            if (LockY)
            {
                transform.position = new Vector3(startPointX + (Cam.position.x - camStartX) * moveRate, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(startPointX + (Cam.position.x - camStartX) * moveRate, startPointY + (Cam.position.y - camStartY) * moveRate, transform.position.z);
            }
        }
    }
}
