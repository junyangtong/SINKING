using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可破碎砖块 - 被玩家攻击后破碎
/// 支持对象池复用
/// </summary>
public class BreakableBlock : MonoBehaviour
{
    [Header("特效设置")]
    public GameObject breakEffect;
    public float destroyDelay = 0.1f;

    [Header("音效设置")]
    public AudioClip breakSound;

    [Header("视觉效果")]
    public Sprite normalSprite;
    public Sprite brokenSprite;
    public bool shakeBeforeBreak = true;
    public float shakeDuration = 0.1f;
    public float shakeIntensity = 0.05f;
    
    [Header("调试")]
    public bool enableDebugLog = false;

    private SpriteRenderer spriteRenderer;
    private bool isBreaking = false;
    private Vector3 originalPosition;
    private Collider2D blockCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockCollider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        ResetState();
    }
    
    /// <summary>
    /// 重置砖块状态（支持对象池复用）
    /// </summary>
    public void ResetState()
    {
        isBreaking = false;
        originalPosition = transform.localPosition;
        
        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
        
        if (blockCollider != null)
        {
            blockCollider.enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("hitBox") && !isBreaking)
        {
            Break();
        }
    }

    /// <summary>
    /// 触发破碎
    /// </summary>
    public void Break()
    {
        if (isBreaking) return;
        
        isBreaking = true;
        originalPosition = transform.localPosition;

        if (enableDebugLog)
            Debug.Log($"BreakableBlock: 开始破碎 {gameObject.name}");

        if (shakeBeforeBreak)
        {
            StartCoroutine(ShakeAndBreak());
        }
        else
        {
            ExecuteBreak();
        }
    }

    private IEnumerator ShakeAndBreak()
    {
        float elapsed = 0f;
        Vector3 shakeOffset = Vector3.zero;

        while (elapsed < shakeDuration)
        {
            shakeOffset.x = Random.Range(-shakeIntensity, shakeIntensity);
            shakeOffset.y = Random.Range(-shakeIntensity, shakeIntensity);
            transform.localPosition = originalPosition + shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        ExecuteBreak();
    }

    private void ExecuteBreak()
    {
        // 播放特效
        if (breakEffect != null)
        {
            GameObject effect = Instantiate(breakEffect, transform.position, Quaternion.identity);
            // 特效自动销毁
            Destroy(effect, 2f);
        }

        // 播放音效
        PlayBreakSound();

        // 更换为破碎贴图
        if (brokenSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = brokenSprite;
        }

        // 禁用碰撞体
        if (blockCollider != null)
        {
            blockCollider.enabled = false;
        }

        // 延迟隐藏（用于对象池回收）
        StartCoroutine(DelayedHide());
    }
    
    /// <summary>
    /// 播放破碎音效
    /// </summary>
    private void PlayBreakSound()
    {
        if (breakSound == null) return;
        
        // 尝试通过AudioManager播放
        GameObject audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
        {
            AudioManager audioManagerScript = audioManager.GetComponent<AudioManager>();
            if (audioManagerScript != null)
            {
                audioManagerScript.PlaySound(breakSound);
                return;
            }
        }
        
        // 如果没有AudioManager，直接播放
        AudioSource.PlayClipAtPoint(breakSound, transform.position);
    }
    
    /// <summary>
    /// 延迟隐藏（用于对象池回收）
    /// </summary>
    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(destroyDelay);
        
        isBreaking = false;
        gameObject.SetActive(false);
    }
}
