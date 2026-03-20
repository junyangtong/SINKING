using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPool : MonoBehaviour
{
    public static MapPool instance;
    public GameObject Row1;
    private float timeRemaining = 6;
    public GameObject Target;
    public int Rowcount;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    void Start()
    {
        
    }
    void Awake()
    {
        instance = this;
        //初始化对象池
        FillPool();
    }
    void Update()
    {

            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                GetFromPool();

                timeRemaining = 6;
            }
            
    }
    public void FillPool()
    {
        for(int i=0; i<Rowcount;i++)
        {
            var newRow = Instantiate(Row1);
            newRow.transform.SetParent(transform);

            //取消启用，返回对象池
            ReturnPool(newRow);
        }
    }

    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);

        availableObjects.Enqueue(gameObject);
    }
    public GameObject GetFromPool()
    {
        if(availableObjects.Count == 0)
        {
            FillPool();
        }
        
        var outRow = availableObjects.Dequeue();
        outRow.SetActive(true);
        foreach(Transform child in outRow.transform)
        {
            child.gameObject.SetActive(true);
        }
        return outRow;
    }
}
