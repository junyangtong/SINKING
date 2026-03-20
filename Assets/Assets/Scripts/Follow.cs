//实现缓慢跟踪某个物体的效果
using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

    public Transform target;
    public Vector3 offset;
    private Vector3 temp;
    public float speed;
    public bool isPortraitMode = false;
    public float fixedYPosition = 0f;

    void Update()
    {
        target = target.GetComponent<Transform>();
        temp = transform.position;
        transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * speed);
        
        if (isPortraitMode)
        {
            transform.position = new Vector3(transform.position.x, fixedYPosition, temp.z);
        }
        else
        {
            transform.position = new Vector3(temp.x, transform.position.y, temp.z);
        }
    }
    
}
