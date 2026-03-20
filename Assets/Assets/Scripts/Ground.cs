using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    //private Animator anim;
    public AnimationClip GroundDisslove;
    public float timeRemaining = 1.8f;
    private bool starCount = false;
    public GameObject NormalObj;
    public GameObject DissloveObj;
    [Header("声音部分")]
    public AudioClip distroy;
    //public float Speed = 1f;
    void OnEnable()
    {
        NormalObj.SetActive(true);
        DissloveObj.SetActive(false);
    }
    private void Update() 
    {
        //transform.position = new Vector3 (transform.position.x, transform.position.y + Speed * Time.deltaTime, transform.position.z);
        if(starCount == true)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                Debug.Log("地面消失！");
                timeRemaining = 1.8f;
                starCount = false;
                GameObject.Find("AudioManager").GetComponent<AudioManager>().PlaySound(distroy);
                gameObject.SetActive(false);
                
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag =="Player1" || other.gameObject.tag =="Player2" || other.gameObject.tag =="Distroy")
        {
            starCount = true;
            DissloveObj.SetActive(true);
            NormalObj.SetActive(false);
        }
        
    }
}
