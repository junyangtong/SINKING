using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 平台生成器 - 适配手机竖屏
/// 生成长条形平台，包含可破碎和不可破碎两种砖块
/// 平台持续向上移动，玩家需要不断跳跃躲避
/// </summary>
public class PlatformGenerator : MonoBehaviour
{
    public static PlatformGenerator instance;

    #region Prefab Settings
    [Header("方块Prefab设置")]
    [Tooltip("可破碎砖块Prefab（包含BreakableBlock组件）")]
    public GameObject breakableBlockPrefab;
    [Tooltip("不可破碎砖块Prefab（包含Ground组件，踩踏后溶解）")]
    public GameObject unbreakableBlockPrefab;
    
    [Header("砖块尺寸设置")]
    [Tooltip("单个砖块的实际尺寸（根据Prefab SpriteRenderer的实际大小设置）")]
    public float blockSize = 1f;
    [Tooltip("砖块之间的间距（防止重叠）")]
    public float blockSpacing = 0.05f;
    #endregion

    #region Platform Settings
    [Header("平台生成设置")]
    [Tooltip("每个平台的砖块数量")]
    public int blocksPerPlatform = 8;
    [Tooltip("平台中空隙出现的概率")]
    [Range(0f, 1f)] public float gapChance = 0.15f;
    [Tooltip("可破碎砖块出现的概率")]
    [Range(0f, 1f)] public float breakableChance = 0.25f;
    [Tooltip("每个平台至少保留的砖块数量（防止空隙太多）")]
    public int minBlocksPerPlatform = 4;
    [Tooltip("两侧边距（防止砖块超出屏幕）")]
    public float sideMargin = 0.3f;
    #endregion

    #region Movement Settings
    [Header("移动设置")]
    [Tooltip("平台向上移动速度")]
    public float moveSpeed = 1f;
    [Tooltip("平台生成间隔时间")]
    public float spawnInterval = 2.5f;
    [Tooltip("平台超出屏幕顶部后的销毁距离")]
    public float destroyYOffset = 2f;
    #endregion

    #region Camera Reference
    [Header("相机引用")]
    public Camera mainCamera;
    #endregion

    // 内部变量
    private Queue<GameObject> platformPool = new Queue<GameObject>();
    private float spawnTimer = 0f;
    private List<GameObject> activePlatforms = new List<GameObject>();
    private float screenLeftEdge;
    private float screenRightEdge;
    private float screenTopEdge;
    private float screenBottomEdge;
    private float platformWidth;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeCamera();
        CalculateScreenBounds();
        CalculatePlatformWidth();
        
        // 预热对象池
        FillPool(3);
        
        // 立即生成第一个平台
        SpawnPlatform();
    }

    void InitializeCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("PlatformGenerator: 未找到相机！");
        }
    }

    /// <summary>
    /// 计算屏幕边界（基于相机orthographicSize和屏幕宽高比）
    /// </summary>
    void CalculateScreenBounds()
    {
        if (mainCamera == null) return;

        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * (Screen.width / (float)Screen.height);

        screenLeftEdge = -horzExtent;
        screenRightEdge = horzExtent;
        screenTopEdge = vertExtent;
        screenBottomEdge = -vertExtent;

        Debug.Log($"PlatformGenerator: 屏幕边界 - 左:{screenLeftEdge:F2}, 右:{screenRightEdge:F2}, 上:{screenTopEdge:F2}, 下:{screenBottomEdge:F2}");
    }

    /// <summary>
    /// 计算平台总宽度
    /// </summary>
    void CalculatePlatformWidth()
    {
        // 平台宽度 = 砖块数量 * (砖块尺寸 + 间距) - 最后一个间距
        platformWidth = blocksPerPlatform * (blockSize + blockSpacing) - blockSpacing;
    }

    void Update()
    {
        // 每帧重新计算屏幕边界（处理屏幕旋转等变化）
        if (Time.frameCount % 60 == 0)
        {
            CalculateScreenBounds();
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnPlatform();
            spawnTimer = 0f;
        }

        MovePlatforms();
        CleanupPlatforms();
    }

    /// <summary>
    /// 填充对象池
    /// </summary>
    void FillPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject platform = CreateNewPlatform();
            platform.SetActive(false);
            platformPool.Enqueue(platform);
        }
    }

    /// <summary>
    /// 创建新平台（程序化生成砖块）
    /// </summary>
    GameObject CreateNewPlatform()
    {
        GameObject platform = new GameObject("Platform");
        platform.transform.SetParent(transform);

        // 计算实际可用的屏幕宽度
        float availableWidth = screenRightEdge - screenLeftEdge - (sideMargin * 2);
        
        // 计算砖块数量（基于屏幕宽度）
        int actualBlockCount = Mathf.FloorToInt(availableWidth / (blockSize + blockSpacing));
        actualBlockCount = Mathf.Max(actualBlockCount, minBlocksPerPlatform);
        
        // 计算平台总宽度
        float totalWidth = actualBlockCount * (blockSize + blockSpacing) - blockSpacing;
        
        // 计算起始X位置（居中对齐）
        float startX = -totalWidth / 2f + blockSize / 2f;

        // 记录已生成的砖块数量，确保最少数量
        int blocksCreated = 0;
        List<int> gapIndices = new List<int>();

        // 第一遍：确定哪些位置会有空隙
        for (int i = 0; i < actualBlockCount; i++)
        {
            if (Random.value < gapChance)
            {
                gapIndices.Add(i);
            }
        }

        // 确保不会生成太多空隙
        while (gapIndices.Count > actualBlockCount - minBlocksPerPlatform && gapIndices.Count > 0)
        {
            gapIndices.RemoveAt(Random.Range(0, gapIndices.Count));
        }

        // 第二遍：生成砖块
        for (int i = 0; i < actualBlockCount; i++)
        {
            // 跳过空隙位置
            if (gapIndices.Contains(i))
                continue;

            // 决定砖块类型
            GameObject prefabToUse = Random.value < breakableChance ? breakableBlockPrefab : unbreakableBlockPrefab;
            if (prefabToUse == null)
            {
                Debug.LogWarning("PlatformGenerator: Prefab未设置！");
                continue;
            }

            // 计算砖块位置（确保无重叠）
            float blockX = startX + i * (blockSize + blockSpacing);
            Vector3 blockPosition = new Vector3(blockX, 0, 0);

            // 实例化砖块
            GameObject block = Instantiate(prefabToUse, blockPosition, Quaternion.identity);
            block.transform.SetParent(platform.transform);
            
            // 确保砖块尺寸正确
            block.transform.localScale = new Vector3(blockSize, blockSize, 1f);
            
            // 重置砖块状态（用于对象池复用）
            block.SetActive(true);

            blocksCreated++;
        }

        // 添加平台移动组件
        PlatformMover mover = platform.GetComponent<PlatformMover>();
        if (mover == null)
        {
            mover = platform.AddComponent<PlatformMover>();
        }
        mover.Initialize(this, moveSpeed);

        return platform;
    }

    /// <summary>
    /// 生成平台
    /// </summary>
    void SpawnPlatform()
    {
        GameObject platform;

        // 从对象池获取或创建新平台
        if (platformPool.Count > 0)
        {
            platform = platformPool.Dequeue();
            platform.SetActive(true);

            // 重新激活所有子砖块
            foreach (Transform child in platform.transform)
            {
                child.gameObject.SetActive(true);
                
                // 重置砖块状态
                Ground ground = child.GetComponent<Ground>();
                if (ground != null)
                {
                    ground.timeRemaining = 1.8f;
                    // 重置Ground状态需要通过反射或公开方法
                    // 暂时假设OnEnable会处理重置
                }
                
                BreakableBlock breakable = child.GetComponent<BreakableBlock>();
                if (breakable != null)
                {
                    // BreakableBlock需要重置状态
                    // 暂时假设会由重新实例化处理
                }
            }
        }
        else
        {
            platform = CreateNewPlatform();
        }

        // 设置平台生成位置（屏幕底部下方）
        float spawnY = screenBottomEdge - 1f;
        
        // 随机X偏移（小幅左右偏移增加趣味性）
        float randomXOffset = Random.Range(-0.5f, 0.5f);
        
        platform.transform.position = new Vector3(randomXOffset, spawnY, 0);
        activePlatforms.Add(platform);
    }

    /// <summary>
    /// 移动所有活动平台（向上移动）
    /// </summary>
    void MovePlatforms()
    {
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            GameObject platform = activePlatforms[i];
            if (platform == null)
            {
                activePlatforms.RemoveAt(i);
                continue;
            }

            // 平台向上移动
            platform.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 清理超出屏幕的平台
    /// </summary>
    void CleanupPlatforms()
    {
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            GameObject platform = activePlatforms[i];
            if (platform == null)
            {
                activePlatforms.RemoveAt(i);
                continue;
            }

            // 平台超出屏幕顶部，回收到对象池
            if (platform.transform.position.y > screenTopEdge + destroyYOffset)
            {
                ReturnPlatform(platform);
            }
        }
    }

    /// <summary>
    /// 回收平台到对象池
    /// </summary>
    public void ReturnPlatform(GameObject platform)
    {
        activePlatforms.Remove(platform);
        platform.SetActive(false);
        platformPool.Enqueue(platform);
    }

    /// <summary>
    /// 获取屏幕边界信息（供其他脚本使用）
    /// </summary>
    public void GetScreenBounds(out float left, out float right, out float top, out float bottom)
    {
        left = screenLeftEdge;
        right = screenRightEdge;
        top = screenTopEdge;
        bottom = screenBottomEdge;
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (mainCamera != null)
        {
            // 绘制屏幕边界
            Gizmos.color = Color.green;
            float vertExtent = mainCamera.orthographicSize;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);
            
            Vector3 bottomLeft = new Vector3(-horzExtent, -vertExtent, 0);
            Vector3 bottomRight = new Vector3(horzExtent, -vertExtent, 0);
            Vector3 topRight = new Vector3(horzExtent, vertExtent, 0);
            Vector3 topLeft = new Vector3(-horzExtent, vertExtent, 0);
            
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
            
            // 绘制生成区域
            Gizmos.color = Color.yellow;
            float spawnY = -vertExtent - 1f;
            Gizmos.DrawLine(new Vector3(-horzExtent, spawnY, 0), new Vector3(horzExtent, spawnY, 0));
        }
    }
    #endif
}

/// <summary>
/// 平台移动组件
/// </summary>
public class PlatformMover : MonoBehaviour
{
    private PlatformGenerator generator;
    private float speed;

    public void Initialize(PlatformGenerator gen, float moveSpeed)
    {
        generator = gen;
        speed = moveSpeed;
    }

    // 注意：实际移动逻辑在PlatformGenerator.MovePlatforms()中统一处理
    // 此组件主要用于扩展功能（如平台特殊效果）
}
