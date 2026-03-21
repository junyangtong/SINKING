using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 段落式平台生成器 - 适配手机竖屏
/// 生成由连续段落组成的平台行，包含可破碎、不可破碎和空隙三种类型
/// 平台持续向上移动，玩家需要不断跳跃躲避
/// </summary>
public class PlatformGenerator : MonoBehaviour
{
    public static PlatformGenerator instance;

    // ====== 砖块类型常量 ======
    public const int BLOCK_BREAKABLE = 1;  // 可破碎
    public const int BLOCK_SOLID = 2;      // 不可破碎
    public const int BLOCK_GAP = 3;        // 空隙

    #region Prefab Settings
    [Header("方块Prefab设置")]
    [Tooltip("可破碎砖块Prefab（踩踏/攻击后溶解消失）")]
    public GameObject breakableBlockPrefab;
    [Tooltip("不可破碎砖块Prefab（永不消失的安全平台）")]
    public GameObject unbreakableBlockPrefab;
    #endregion

    #region Block Size Settings
    [Header("砖块尺寸设置")]
    [Tooltip("单个砖块的实际尺寸")]
    public float blockSize = 0.5f;
    [Tooltip("砖块之间的间距（建议设为0避免角色卡缝）")]
    public float blockSpacing = 0f;
    [Tooltip("两侧边距（防止砖块超出屏幕）")]
    public float sideMargin = 0.3f;
    #endregion

    #region Segment Generation Settings
    [Header("段落生成权重")]
    [Tooltip("可破碎砖块段的权重")]
    [Range(0f, 100f)] public float breakableWeight = 35f;
    [Tooltip("不可破碎砖块段的权重")]
    [Range(0f, 100f)] public float solidWeight = 40f;
    [Tooltip("空隙段的权重")]
    [Range(0f, 100f)] public float gapWeight = 25f;

    [Header("段落长度设置")]
    [Tooltip("砖块段最小长度")]
    [Range(1, 10)] public int segmentMinLength = 1;
    [Tooltip("砖块段最大长度")]
    [Range(1, 10)] public int segmentMaxLength = 5;
    [Tooltip("空隙段最小长度")]
    [Range(1, 5)] public int gapMinLength = 2;
    [Tooltip("空隙段最大长度")]
    [Range(1, 5)] public int gapMaxLength = 3;
    [Tooltip("单行内连续空隙最大长度（约束上限）")]
    [Range(1, 6)] public int maxGapLength = 3;
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

    // ====== 内部变量 ======
    private Queue<GameObject> platformPool = new Queue<GameObject>();
    private float spawnTimer = 0f;
    private List<GameObject> activePlatforms = new List<GameObject>();
    private float screenLeftEdge;
    private float screenRightEdge;
    private float screenTopEdge;
    private float screenBottomEdge;

    // 行间空隙防对齐：记录上一行的空隙位置
    private List<Vector2Int> lastRowGapRanges = new List<Vector2Int>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeCamera();
        CalculateScreenBounds();

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
    /// 计算一行可容纳的砖块数量
    /// </summary>
    int CalculateBlocksPerRow()
    {
        float availableWidth = screenRightEdge - screenLeftEdge - (sideMargin * 2);
        int count = Mathf.FloorToInt(availableWidth / (blockSize + blockSpacing));
        return Mathf.Max(count, 4); // 至少4块
    }

    void Update()
    {
        // 每60帧重新计算屏幕边界（处理屏幕旋转等变化）
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

    // ================================================================
    //  段落式行序列生成算法
    // ================================================================

    /// <summary>
    /// 生成一行平台的砖块类型序列
    /// 使用段落式算法：逐段填充，同类型砖块连续排列
    /// </summary>
    /// <param name="blockCount">一行的砖块数量</param>
    /// <returns>int[] 序列，元素为 BLOCK_BREAKABLE(1)/BLOCK_SOLID(2)/BLOCK_GAP(3)</returns>
    int[] GenerateRowPattern(int blockCount)
    {
        int[] pattern = new int[blockCount];
        int cursor = 0;
        int lastSegmentType = 0; // 上一段的类型，0表示尚无

        while (cursor < blockCount)
        {
            // 选择段类型
            int segType = PickSegmentType(lastSegmentType);

            // 选择段长度
            int segLen;
            if (segType == BLOCK_GAP)
            {
                segLen = Random.Range(gapMinLength, gapMaxLength + 1);
            }
            else
            {
                segLen = Random.Range(segmentMinLength, segmentMaxLength + 1);
            }

            // 不超出行长度
            segLen = Mathf.Min(segLen, blockCount - cursor);

            // 填入序列
            for (int i = 0; i < segLen; i++)
            {
                pattern[cursor + i] = segType;
            }

            cursor += segLen;
            lastSegmentType = segType;
        }

        // 约束后处理
        ApplyConstraints(pattern);

        // 行间空隙防对齐
        ApplyInterRowGapCheck(pattern);

        // 更新上一行的空隙记录
        UpdateLastRowGapRanges(pattern);

        return pattern;
    }

    /// <summary>
    /// 按权重选择段类型（空隙段后不能紧跟空隙段）
    /// </summary>
    int PickSegmentType(int lastType)
    {
        float totalWeight;
        float roll;

        if (lastType == BLOCK_GAP)
        {
            // 上一段是空隙，只能选砖块类型
            totalWeight = breakableWeight + solidWeight;
            roll = Random.Range(0f, totalWeight);
            return roll < breakableWeight ? BLOCK_BREAKABLE : BLOCK_SOLID;
        }
        else
        {
            totalWeight = breakableWeight + solidWeight + gapWeight;
            roll = Random.Range(0f, totalWeight);
            if (roll < breakableWeight)
                return BLOCK_BREAKABLE;
            else if (roll < breakableWeight + solidWeight)
                return BLOCK_SOLID;
            else
                return BLOCK_GAP;
        }
    }

    /// <summary>
    /// 约束后处理：行首行尾不可为空隙；连续空隙不超过 maxGapLength
    /// </summary>
    void ApplyConstraints(int[] pattern)
    {
        int len = pattern.Length;
        if (len == 0) return;

        // 1. 行首不可为空隙 — 将行首的连续空隙全部替换
        for (int i = 0; i < len && pattern[i] == BLOCK_GAP; i++)
        {
            pattern[i] = Random.value < 0.5f ? BLOCK_BREAKABLE : BLOCK_SOLID;
        }

        // 2. 行尾不可为空隙 — 将行尾的连续空隙全部替换
        for (int i = len - 1; i >= 0 && pattern[i] == BLOCK_GAP; i--)
        {
            pattern[i] = Random.value < 0.5f ? BLOCK_BREAKABLE : BLOCK_SOLID;
        }

        // 3. 连续空隙不超过 maxGapLength
        int consecutiveGaps = 0;
        for (int i = 0; i < len; i++)
        {
            if (pattern[i] == BLOCK_GAP)
            {
                consecutiveGaps++;
                if (consecutiveGaps > maxGapLength)
                {
                    // 超出部分替换为砖块
                    pattern[i] = Random.value < 0.5f ? BLOCK_BREAKABLE : BLOCK_SOLID;
                    consecutiveGaps = 0;
                }
            }
            else
            {
                consecutiveGaps = 0;
            }
        }
    }

    /// <summary>
    /// 行间空隙防对齐：检查新行的空隙是否与上一行完全重叠
    /// </summary>
    void ApplyInterRowGapCheck(int[] pattern)
    {
        if (lastRowGapRanges.Count == 0) return;

        int len = pattern.Length;

        // 找出新行的空隙区间
        List<Vector2Int> newGapRanges = ExtractGapRanges(pattern);

        foreach (var newGap in newGapRanges)
        {
            foreach (var lastGap in lastRowGapRanges)
            {
                // 检查新空隙是否完全被上一行空隙包含
                if (newGap.x >= lastGap.x && newGap.y <= lastGap.y)
                {
                    // 完全重叠！将新行的这段空隙替换为砖块
                    for (int i = newGap.x; i <= newGap.y && i < len; i++)
                    {
                        pattern[i] = Random.value < 0.5f ? BLOCK_BREAKABLE : BLOCK_SOLID;
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 提取序列中的空隙区间列表 [start, end] (inclusive)
    /// </summary>
    List<Vector2Int> ExtractGapRanges(int[] pattern)
    {
        List<Vector2Int> ranges = new List<Vector2Int>();
        int i = 0;
        while (i < pattern.Length)
        {
            if (pattern[i] == BLOCK_GAP)
            {
                int start = i;
                while (i < pattern.Length && pattern[i] == BLOCK_GAP)
                    i++;
                ranges.Add(new Vector2Int(start, i - 1));
            }
            else
            {
                i++;
            }
        }
        return ranges;
    }

    /// <summary>
    /// 更新上一行的空隙位置记录
    /// </summary>
    void UpdateLastRowGapRanges(int[] pattern)
    {
        lastRowGapRanges = ExtractGapRanges(pattern);
    }

    // ================================================================
    //  对象池 & 平台生成
    // ================================================================

    /// <summary>
    /// 填充对象池（预创建空的父容器）
    /// </summary>
    void FillPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject platform = new GameObject("Platform");
            platform.transform.SetParent(transform);
            
            PlatformMover mover = platform.AddComponent<PlatformMover>();
            mover.Initialize(this, moveSpeed);
            
            platform.SetActive(false);
            platformPool.Enqueue(platform);
        }
    }

    /// <summary>
    /// 为平台填充砖块（根据段落序列实例化）
    /// </summary>
    void PopulatePlatform(GameObject platform)
    {
        int blocksPerRow = CalculateBlocksPerRow();
        int[] pattern = GenerateRowPattern(blocksPerRow);

        // 计算平台总宽度和起始X
        float totalWidth = blocksPerRow * (blockSize + blockSpacing) - blockSpacing;
        float startX = -totalWidth / 2f + blockSize / 2f;

        for (int i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] == BLOCK_GAP)
                continue; // 空隙，跳过

            // 根据类型选择Prefab
            GameObject prefabToUse = (pattern[i] == BLOCK_BREAKABLE) ? breakableBlockPrefab : unbreakableBlockPrefab;

            if (prefabToUse == null)
            {
                Debug.LogWarning($"PlatformGenerator: 类型{pattern[i]}的Prefab未设置！");
                continue;
            }

            // 计算砖块位置
            float blockX = startX + i * (blockSize + blockSpacing);
            Vector3 blockPosition = new Vector3(blockX, 0, 0);

            // 实例化砖块
            GameObject block = Instantiate(prefabToUse, blockPosition, Quaternion.identity);
            block.transform.SetParent(platform.transform);
            block.transform.localPosition = blockPosition;

            // 确保砖块尺寸正确
            block.transform.localScale = new Vector3(blockSize, blockSize, 1f);
            block.SetActive(true);
        }
    }

    /// <summary>
    /// 清除平台上所有子砖块
    /// </summary>
    void ClearPlatformChildren(GameObject platform)
    {
        for (int i = platform.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(platform.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 生成平台
    /// </summary>
    void SpawnPlatform()
    {
        GameObject platform;

        if (platformPool.Count > 0)
        {
            platform = platformPool.Dequeue();
            platform.SetActive(true);
            
            // 清除旧的子砖块
            ClearPlatformChildren(platform);
        }
        else
        {
            platform = new GameObject("Platform");
            platform.transform.SetParent(transform);
            
            PlatformMover mover = platform.AddComponent<PlatformMover>();
            mover.Initialize(this, moveSpeed);
        }

        // 按新的段落序列填充砖块
        PopulatePlatform(platform);

        // 设置平台生成位置（屏幕底部下方）
        float spawnY = screenBottomEdge - 1f;

        // 小幅随机X偏移
        float randomXOffset = Random.Range(-0.3f, 0.3f);

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

            if (platform.transform.position.y > screenTopEdge + destroyYOffset)
            {
                ReturnPlatform(platform);
            }
        }
    }

    /// <summary>
    /// 回收平台到对象池（销毁子砖块，保留父容器）
    /// </summary>
    public void ReturnPlatform(GameObject platform)
    {
        activePlatforms.Remove(platform);
        ClearPlatformChildren(platform);
        platform.SetActive(false);
        platformPool.Enqueue(platform);
    }

    // ================================================================
    //  难度接口
    // ================================================================

    /// <summary>
    /// 运行时调整难度参数（供外部系统调用）
    /// </summary>
    /// <param name="newBreakableWeight">可破碎砖块权重</param>
    /// <param name="newSolidWeight">不可破碎砖块权重</param>
    /// <param name="newGapWeight">空隙权重</param>
    public void SetDifficultyParams(float newBreakableWeight, float newSolidWeight, float newGapWeight)
    {
        breakableWeight = Mathf.Max(0f, newBreakableWeight);
        solidWeight = Mathf.Max(0f, newSolidWeight);
        gapWeight = Mathf.Max(0f, newGapWeight);
        
        Debug.Log($"PlatformGenerator: 难度参数更新 - 可破碎:{breakableWeight}, 不可破碎:{solidWeight}, 空隙:{gapWeight}");
    }

    /// <summary>
    /// 运行时调整段长度参数
    /// </summary>
    public void SetSegmentLengthParams(int minLen, int maxLen, int gapMin, int gapMax, int maxGap)
    {
        segmentMinLength = Mathf.Max(1, minLen);
        segmentMaxLength = Mathf.Max(segmentMinLength, maxLen);
        gapMinLength = Mathf.Max(1, gapMin);
        gapMaxLength = Mathf.Max(gapMinLength, gapMax);
        maxGapLength = Mathf.Max(1, maxGap);
    }

    // ================================================================
    //  公共工具方法
    // ================================================================

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
