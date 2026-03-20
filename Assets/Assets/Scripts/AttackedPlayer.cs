using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedPlayer : MonoBehaviour
{
    private Rigidbody2D rigi;
    private Animator anim;
    private bool Attacked = false;

    [Header("声音部分")]
    private AudioSource audioSource;
    public AudioClip attacked;
    void Start()
    {
        rigi = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        //声音部分
        audioSource = transform.GetComponent<AudioSource>();
    }
    //播放音效
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag =="Player1HitBox")
        {   Attacked = true;
            PlaySound(attacked);
            anim.ResetTrigger("Attacked");
            if(other.gameObject.transform.position.x > gameObject.transform.position.x)
            {
                rigi.AddForce(new Vector2(-3,0),ForceMode2D.Impulse);            Debug.Log("左");
                anim.SetTrigger("Attacked");
            }
            else
            {
                rigi.AddForce(new Vector2(3,0),ForceMode2D.Impulse);            Debug.Log("右!");
                anim.SetTrigger("Attacked");
            }
            
        }
        if(other.gameObject.tag =="Player2HitBox")
        {   Attacked = true;
            PlaySound(attacked);
            anim.SetTrigger("Attacked");
            if(other.gameObject.transform.position.x > gameObject.transform.position.x)
            {
                rigi.AddForce(new Vector2(-3,0),ForceMode2D.Impulse);            Debug.Log("右");
                anim.SetTrigger("Attacked");
            }
            else
            {
                rigi.AddForce(new Vector2(3,0),ForceMode2D.Impulse);            Debug.Log("左!");
                anim.SetTrigger("Attacked");
            }
            
        }
        

    }


}
