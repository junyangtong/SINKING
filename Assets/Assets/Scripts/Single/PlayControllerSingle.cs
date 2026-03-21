using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayControllerSingle : MonoBehaviour
{
    private int playerIndex;
    private Rigidbody2D rigi;
    private Animator anim;

    int moveChangeAni;

    public float MoveSpeed =  10f;
    public float GhostMoveSpeed =  10f;
    Vector2 Move;
    private float moveX;
    private bool isMove;
    private bool runDir;
    private bool isDeath = false;

    private bool Attacking = false;

    public float jumpHeight = 5f;

    private int jumpCount;

    private bool isOnGround;
    private bool jumpis = false;
    public GameObject GameOverUI; //GameOverUI
    public GameObject StopGameUI; //StopGameUI
    public GameObject hitBox;
    [Header("道具相关")]
    private float Restartime = 4; //重生时间
    public float Smallertime = 5; //道具生效使时间
    public float Fastertime = 5; //道具生效使时间
    private bool isDontDie = false;
    private bool isSmaller = false;
    private bool isFaster = false;
    
    [Header("死亡记录")]
    public int Player1DiedNum = 0;
    public int Player2DiedNum = 0;
    [Header("声音部分")]
    private AudioSource audioSource;
    public AudioClip pick;
    
    [Header("相机边界")]
    public Camera mainCamera;
    
    // 动态计算的屏幕边界
    private float screenLeftEdge;
    private float screenRightEdge;
    private float screenTopEdge;
    private float screenBottomEdge;
    
    // 玩家边界安全边距
    public float boundaryMargin = 0.3f;


    void Start()
    {
        playerIndex = 0;
        GameOverUI.SetActive(false);              //GameOverUI默认不显示
        rigi = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        //声音部分
        audioSource = transform.GetComponent<AudioSource>();
        
        // 初始化相机边界
        InitializeCameraBounds();
    }
    
    /// <summary>
    /// 初始化相机边界（动态计算，适配竖屏）
    /// </summary>
    private void InitializeCameraBounds()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            float vertExtent = mainCamera.orthographicSize;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);
            
            screenLeftEdge = -horzExtent;
            screenRightEdge = horzExtent;
            screenTopEdge = vertExtent;
            screenBottomEdge = -vertExtent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Player1DiedNum == 1)
        {
            Time.timeScale = 0;
            StopGameUI.SetActive(false);
            GameOverUI.SetActive(true);
            Player1DiedNum+=1;
        }
        

        Movement();
        Jump();
        Attack();
        Revive();
        Smaller();
        Faster();
        anim.SetBool("IsDeath", isDeath);
        //DeathTimer();
    }
    //播放背景音乐(不能重叠)
    public void PlayMusic(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
    //播放音效
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    [Header("移动手感")]
    [Tooltip("地面摩擦减速系数（越大减速越快，0=无摩擦）")]
    public float groundFriction = 8f;
    [Tooltip("空中摩擦减速系数（比地面小，保留更多惯性）")]
    public float airFriction = 2f;

    //移动
    private void Movement()
    {
        // 死亡状态不执行移动（避免对 Static body 设置 velocity）
        if (isDeath) return;

        // 使用连续输入值（-1.0 ~ 1.0），比离散的 -1/0/1 更平滑
        moveX = SwipeInputManager.Instance.GetHorizontalAxis();
        isMove = Mathf.Abs(moveX) > 0.05f;

        float currentVelX = rigi.velocity.x;
        float targetVelocityX = moveX * MoveSpeed;

        if (isMove)
        {
            // 有输入时：快速趋近目标速度
            currentVelX = Mathf.Lerp(currentVelX, targetVelocityX, Time.deltaTime * 15f);
        }
        else
        {
            // 无输入时：根据地面/空中状态自然减速（保留惯性）
            float friction = isOnGround ? groundFriction : airFriction;
            currentVelX = Mathf.Lerp(currentVelX, 0f, Time.deltaTime * friction);
            // 速度很小时直接归零
            if (Mathf.Abs(currentVelX) < 0.1f) currentVelX = 0f;
        }

        rigi.velocity = new Vector2(currentVelX, rigi.velocity.y);

        // 动画方向判定（带小死区防止频繁切换）
        if(moveX > 0.1f){
            runDir = false;
            anim.SetBool("Middle", false);
        }
        else if(moveX < -0.1f){
            runDir = true;
            anim.SetBool("Middle", false);
        }
        else{
            anim.SetBool("Middle", true);
        }
        anim.SetFloat("Blend", moveX);
        anim.SetBool("runDir", runDir);
        anim.SetBool("isMove", isMove);

        // 动态边界检测（适配竖屏）
        ClampPlayerPosition();
    }
    
    /// <summary>
    /// 限制玩家位置在屏幕范围内
    /// </summary>
    private void ClampPlayerPosition()
    {
        // 每帧更新边界（处理屏幕旋转等情况）
        if (mainCamera != null)
        {
            float vertExtent = mainCamera.orthographicSize;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);
            
            screenLeftEdge = -horzExtent;
            screenRightEdge = horzExtent;
            screenTopEdge = vertExtent;
            screenBottomEdge = -vertExtent;
        }
        
        // 限制玩家位置，考虑边界安全边距
        float minX = screenLeftEdge + boundaryMargin;
        float maxX = screenRightEdge - boundaryMargin;
        float minY = screenBottomEdge + boundaryMargin;
        float maxY = screenTopEdge - boundaryMargin;
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
    //跳跃
    private void Jump()
    {
        if (SwipeInputManager.Instance.GetJumpTrigger(playerIndex) && jumpis)
        {
            rigi.velocity = new Vector2(rigi.velocity.x,jumpHeight);
            anim.SetBool("Jump", true);
            anim.SetBool("Down", false);
        }
        if(rigi.velocity.y<-1.5)
            anim.SetBool("Down", true);
        else
            anim.SetBool("Down", false);
    }
    //地面检测
    private void OnCollisionEnter2D(Collision2D col)
    {
        Grounding(col,false);
        jumpis = false;
        anim.SetBool("Jump", false);
        if (col.gameObject.tag == "Death")
        {
            Death();
        }
        if (col.gameObject.tag == "DontDie" && isDeath == false)
        {
            isDontDie=true;
            PlaySound(pick);
            anim.SetBool("isDontDie", isDontDie);
            DontDie();
            col.gameObject.SetActive(false);
        }
        if (col.gameObject.tag == "Smaller" && isDeath == false)
        {
            isSmaller=true;
            PlaySound(pick);
            col.gameObject.SetActive(false);
        }
        if (col.gameObject.tag == "Faster" && isDeath == false)
        {
            isFaster=true;
            PlaySound(pick);
            col.gameObject.SetActive(false);
        }
    }
    //角色能被惩罚杀死
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Distroy")
        {
            Debug.Log("被punish攻击！");
            Death();
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Grounding(collision,false);
        if (collision.gameObject.tag == "Ground")
        {
        jumpis = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Grounding(other,true);
        jumpis = false;
    }

    private void Grounding(Collision2D col,bool exitState)
    {
        if (exitState)                                      //离开为真
        {
            if (col.gameObject.layer ==LayerMask.NameToLayer("Terrain"))
            {
                isOnGround = false;
            }
        }
        else
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Terrain")&&!isOnGround&&col.contacts[0].normal==Vector2.up)
            {
                isOnGround = true;
            }
            else if(col.gameObject.layer == LayerMask.NameToLayer("Terrain")&&!isOnGround&&col.contacts[0].normal==Vector2.down)
            {  
            }
        }
    }
    private void Attack()
    {
        Attacking = false;
        hitBox.SetActive(false);
        if (SwipeInputManager.Instance.GetAttackTrigger(playerIndex))
        {
            Attacking = true;
            hitBox.SetActive(true);
        }
        else
        {
            hitBox.SetActive(false);
        }
        anim.SetBool("Attacking", Attacking);
    }

    
    //死亡
    private void Death()
    {
        rigi.bodyType = RigidbodyType2D.Static;
        isDeath = true;
        if (gameObject.CompareTag("Player1"))
        {
            if(!isDontDie)
            Player1DiedNum++;
            else
            {
                isDontDie=false;anim.SetBool("isDontDie", isDontDie);
            }
            
        }
        if (gameObject.CompareTag("Player2"))
        {
            if(!isDontDie)
            Player2DiedNum++;
            else
            {
                isDontDie=false;anim.SetBool("isDontDie", isDontDie);
            }
        }
        
        //GameOverUI.SetActive(true);
        //anim.Play("Death");
    }
    //复活
    private void Revive()
    {
        if(isDeath == true)
        {
            int horizontalDir = SwipeInputManager.Instance.GetHorizontalDirection(playerIndex);
            SwipeDirection swipeDir = SwipeInputManager.Instance.GetSwipeDirection(playerIndex);
            
            if (horizontalDir == 1)
            {
                gameObject.transform.position += new Vector3(GhostMoveSpeed * Time.deltaTime, 0, 0);
            }
            if (horizontalDir == -1)
            {
                gameObject.transform.position += new Vector3(-GhostMoveSpeed * Time.deltaTime, 0, 0);
            }
            if (swipeDir == SwipeDirection.Up)
            {
                gameObject.transform.position += new Vector3(0, GhostMoveSpeed * Time.deltaTime, 0);
            }
            if (swipeDir == SwipeDirection.Down)
            {
                gameObject.transform.position += new Vector3(0, -GhostMoveSpeed * Time.deltaTime, 0);
            }
            
            // 使用动态边界限制幽灵移动范围
            float tempY = Mathf.Clamp(transform.position.y, screenBottomEdge + boundaryMargin, screenTopEdge - boundaryMargin);
            float tempX = Mathf.Clamp(transform.position.x, screenLeftEdge + boundaryMargin, screenRightEdge - boundaryMargin);
            gameObject.transform.position = new Vector3(tempX, tempY, 0);
            
            if (Restartime > 0)
            {
                Restartime -= Time.deltaTime;
            }
            else
            {
                
                rigi.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log("复活！");
                isDeath = false;
                Restartime = 4;
            }
        }
        
        
        //GameOverUI.SetActive(true);
    }
    
    //重新加载场景
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    //道具技能
    private void DontDie()
    {
        if(isDontDie==true)
        Debug.Log("免死一次！");
    }
    private void Smaller()
    {
        if(isSmaller==true)
        {
            Debug.Log("变小！");
            transform.localScale = new Vector3(0.2f,0.2f,0.2f);
            rigi.mass = 0.06f;
            if (Smallertime > 0)
            {
                Smallertime -= Time.deltaTime;
            }
            else
            {
                transform.localScale = new Vector3(0.6f,0.6f,0.6f);
                rigi.mass = 0.09f;
                isSmaller = false;
                Smallertime = 5;
            }  
        }
    }
    private void Faster()
    {
        if(isFaster==true)
        {
            Debug.Log("加速！");
            rigi.mass = 0.03f;
            if (Fastertime > 0)
            {
                Fastertime -= Time.deltaTime;
            }
            else
            {
                rigi.mass = 0.09f;
                isFaster = false;
                Fastertime = 5;
            }  
        }
        
    }


}
