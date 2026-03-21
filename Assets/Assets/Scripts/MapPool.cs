using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPool : MonoBehaviour
{
    [Header("已弃用 - 请使用PlatformGenerator")]
    [Tooltip("此脚本已被PlatformGenerator替代，请禁用此组件")]
    public bool isDeprecated = true;

    public static MapPool instance;
    public GameObject Row1;
    private float timeRemaining = 6;
    public GameObject Target;
    public int Rowcount;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    void Start()
    {
        if (isDeprecated)
        {
            Debug.LogWarning("MapPool: 此脚本已弃用，请使用PlatformGenerator替代。");
            enabled = false;
            return;
        }
    }
    void Awake()
    {
        if (isDeprecated) return;
        
        instance = this;
        FillPool();
    }
    void Update()
    {
        if (isDeprecated) return;

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
        if (isDeprecated) return;
        
        for(int i=0; i<Rowcount;i++)
        {
            var newRow = Instantiate(Row1);
            newRow.transform.SetParent(transform);
            ReturnPool(newRow);
        }
    }

    public void ReturnPool(GameObject gameObject)
    {
        if (isDeprecated) return;
        
        gameObject.SetActive(false);
        availableObjects.Enqueue(gameObject);
    }
    public GameObject GetFromPool()
    {
        if (isDeprecated) return null;
        
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
