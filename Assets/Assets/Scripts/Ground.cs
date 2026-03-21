using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地面砖块 - 玩家踩踏后溶解消失
/// 支持对象池复用
/// </summary>
public class Ground : MonoBehaviour
{
    [Header("溶解动画")]
    public AnimationClip GroundDisslove;
    public float dissolveTime = 1.8f;
    [System.NonSerialized]
    public float timeRemaining;
    private bool isDissolving = false;
    
    [Header("视觉对象")]
    public GameObject NormalObj;
    public GameObject DissloveObj;
    
    [Header("音效")]
    public AudioClip destroySound;
    
    [Header("调试")]
    public bool enableDebugLog = false;

    /// <summary>
    /// 对象激活时重置状态（支持对象池复用）
    /// </summary>
    void OnEnable()
    {
        ResetState();
    }
    
    /// <summary>
    /// 重置砖块状态
    /// </summary>
    public void ResetState()
    {
        timeRemaining = dissolveTime;
        isDissolving = false;
        
        if (NormalObj != null)
            NormalObj.SetActive(true);
        if (DissloveObj != null)
            DissloveObj.SetActive(false);
    }

    void Update()
    {
        // 溶解计时
        if (isDissolving)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                CompleteDissolve();
            }
        }
    }
    
    /// <summary>
    /// 开始溶解（玩家踩踏触发）
    /// </summary>
    public void StartDissolve()
    {
        if (isDissolving) return;
        
        isDissolving = true;
        timeRemaining = dissolveTime;
        
        if (DissloveObj != null)
            DissloveObj.SetActive(true);
        if (NormalObj != null)
            NormalObj.SetActive(false);
            
        if (enableDebugLog)
            Debug.Log($"Ground: 开始溶解 {gameObject.name}");
    }
    
    /// <summary>
    /// 完成溶解，隐藏砖块
    /// </summary>
    private void CompleteDissolve()
    {
        if (enableDebugLog)
            Debug.Log($"Ground: 溶解完成 {gameObject.name}");
        
        // 播放音效
        PlayDestroySound();
        
        // 禁用砖块（回收到对象池）
        isDissolving = false;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 播放销毁音效
    /// </summary>
    private void PlayDestroySound()
    {
        if (destroySound == null) return;
        
        // 尝试通过AudioManager播放
        GameObject audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
        {
            AudioManager audioManagerScript = audioManager.GetComponent<AudioManager>();
            if (audioManagerScript != null)
            {
                audioManagerScript.PlaySound(destroySound);
                return;
            }
        }
        
        // 如果没有AudioManager，直接播放
        AudioSource.PlayClipAtPoint(destroySound, transform.position);
    }

    /// <summary>
    /// 玩家踩踏检测 & 攻击破碎检测
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 玩家或惩罚物体触发溶解
        if (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("Distroy"))
        {
            StartDissolve();
        }
        
        // 被玩家攻击(hitBox)时也触发溶解/破碎
        if (other.CompareTag("hitBox"))
        {
            if (enableDebugLog)
                Debug.Log($"Ground: 被攻击破碎 {gameObject.name}");
            StartDissolve();
        }
    }
}
