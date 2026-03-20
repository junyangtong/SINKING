using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowsGenerate : MonoBehaviour
{
    public GameObject ground;
    public int MaxRows = 10;
    public GameObject Rowcontainer;//父物体
    public GameObject Colcontainer;//父物体
    public GameObject Target;
    public float jiange = 1;
    Vector2 nowpos = new Vector2(12,-6);
    void Start()
    {
        for(int row = 0;row < MaxRows;row++)
        {
            GameObject a = Instantiate(ground,transform);
            a.transform.position = nowpos;
            nowpos = new Vector2(nowpos.x+jiange,nowpos.y);
            a.transform.parent = Rowcontainer.transform;
        }
        //for(int colum = 0;colum < Maxcolumns;colum++)
        //{
            
        //}
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
