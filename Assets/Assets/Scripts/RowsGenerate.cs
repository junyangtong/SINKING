using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowsGenerate : MonoBehaviour
{
    [Header("已弃用 - 请使用PlatformGenerator")]
    [Tooltip("此脚本已被PlatformGenerator替代，请禁用此组件")]
    public bool isDeprecated = true;
    
    public GameObject ground;
    public int MaxRows = 10;
    public GameObject Rowcontainer;
    public GameObject Colcontainer;
    public GameObject Target;
    public float jiange = 1;
    Vector2 nowpos = new Vector2(12,-6);
    
    void Start()
    {
        if (isDeprecated)
        {
            Debug.LogWarning("RowsGenerate: 此脚本已弃用，请使用PlatformGenerator替代。如需使用旧逻辑，请取消勾选isDeprecated。");
            return;
        }
        
        for(int row = 0; row < MaxRows; row++)
        {
            GameObject a = Instantiate(ground, transform);
            a.transform.position = nowpos;
            nowpos = new Vector2(nowpos.x + jiange, nowpos.y);
            a.transform.parent = Rowcontainer.transform;
        }
    }

    void Update()
    {

    }
}
