using UnityEngine;

public enum SwipeDirection
{
    None,
    Left,
    Right,
    Up,
    Down
}

public class SwipeInputManager : MonoBehaviour
{
    public static SwipeInputManager Instance { get; private set; }

    [Header("Swipe Settings")]
    [Tooltip("触发滑动手势的最小距离（像素）")]
    [SerializeField] private float minSwipeDistance = 50f;
    [Tooltip("水平移动的死区（像素），低于此值视为不移动")]
    [SerializeField] private float horizontalDeadZone = 5f;
    [Tooltip("水平输入平滑速度（越大越灵敏，越小越平滑）")]
    [SerializeField] private float horizontalSmoothSpeed = 20f;

    // 触控状态
    private int activeTouchId = -1;           // 当前追踪的触摸ID
    private Vector2 touchStartPosition;       // 触摸起始位置
    private Vector2 touchCurrentPosition;     // 触摸当前位置
    private bool isTouching = false;

    // 滑动手势（单次触发）
    private SwipeDirection lastSwipeDirection = SwipeDirection.None;
    private bool jumpTriggered = false;
    private bool attackTriggered = false;

    // 水平持续输入（-1 ~ 1 连续值）
    private float rawHorizontalInput = 0f;
    private float smoothHorizontalInput = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 每帧重置单次触发
        jumpTriggered = false;
        attackTriggered = false;
        lastSwipeDirection = SwipeDirection.None;

        HandleTouchInput();
        HandleMouseInput();

        // 平滑水平输入
        smoothHorizontalInput = Mathf.Lerp(smoothHorizontalInput, rawHorizontalInput, Time.deltaTime * horizontalSmoothSpeed);

        // 接近0时直接归零，避免微小漂移
        if (Mathf.Abs(smoothHorizontalInput) < 0.01f)
            smoothHorizontalInput = 0f;
    }

    /// <summary>
    /// 处理触屏输入（全屏响应，不分区域）
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            if (isTouching)
            {
                isTouching = false;
                activeTouchId = -1;
                rawHorizontalInput = 0f;
            }
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (!isTouching)
                    {
                        activeTouchId = touch.fingerId;
                        touchStartPosition = touch.position;
                        touchCurrentPosition = touch.position;
                        isTouching = true;
                        rawHorizontalInput = 0f;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (touch.fingerId == activeTouchId)
                    {
                        touchCurrentPosition = touch.position;
                        UpdateHorizontalInput();
                        DetectSwipeGesture();
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == activeTouchId)
                    {
                        touchCurrentPosition = touch.position;
                        DetectSwipeGesture();
                        isTouching = false;
                        activeTouchId = -1;
                        rawHorizontalInput = 0f;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 编辑器中用鼠标模拟触屏
    /// </summary>
    private void HandleMouseInput()
    {
#if UNITY_EDITOR
        if (Input.touchCount > 0) return; // 有真实触摸时不用鼠标

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPosition = Input.mousePosition;
            touchCurrentPosition = Input.mousePosition;
            isTouching = true;
            rawHorizontalInput = 0f;
        }
        else if (Input.GetMouseButton(0) && isTouching)
        {
            touchCurrentPosition = Input.mousePosition;
            UpdateHorizontalInput();
            DetectSwipeGesture();
        }
        else if (Input.GetMouseButtonUp(0) && isTouching)
        {
            touchCurrentPosition = Input.mousePosition;
            DetectSwipeGesture();
            isTouching = false;
            rawHorizontalInput = 0f;
        }
#endif
    }

    /// <summary>
    /// 更新水平连续输入（-1 到 1 的连续值）
    /// </summary>
    private void UpdateHorizontalInput()
    {
        float deltaX = touchCurrentPosition.x - touchStartPosition.x;

        if (Mathf.Abs(deltaX) < horizontalDeadZone)
        {
            rawHorizontalInput = 0f;
        }
        else
        {
            // 将偏移映射到 -1 ~ 1 范围，minSwipeDistance 为满值点
            float normalizedInput = (deltaX - Mathf.Sign(deltaX) * horizontalDeadZone) / minSwipeDistance;
            rawHorizontalInput = Mathf.Clamp(normalizedInput, -1f, 1f);
        }
    }

    /// <summary>
    /// 检测单次滑动手势（上滑跳跃、下滑攻击）
    /// </summary>
    private void DetectSwipeGesture()
    {
        Vector2 swipeVector = touchCurrentPosition - touchStartPosition;
        float swipeDistance = swipeVector.magnitude;

        if (swipeDistance >= minSwipeDistance)
        {
            float absX = Mathf.Abs(swipeVector.x);
            float absY = Mathf.Abs(swipeVector.y);

            // 垂直分量大于水平分量的一半即可触发上下手势
            // 允许斜上方/斜下方滑动也能触发跳跃/攻击
            if (absY > absX * 0.5f)
            {
                if (swipeVector.y > 0)
                {
                    lastSwipeDirection = SwipeDirection.Up;
                    jumpTriggered = true;
                }
                else
                {
                    lastSwipeDirection = SwipeDirection.Down;
                    attackTriggered = true;
                }
                // 手势触发后重置起始点
                touchStartPosition = touchCurrentPosition;
            }
            else if (absX > absY)
            {
                lastSwipeDirection = swipeVector.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
        }
    }

    // ================================================================
    //  公共接口 — 兼容旧代码的 playerIndex 参数（单人模式忽略）
    // ================================================================

    public SwipeDirection GetSwipeDirection(int playerIndex = 0)
    {
        return lastSwipeDirection;
    }

    /// <summary>
    /// 获取水平方向输入（-1, 0, 1 离散值，兼容旧代码）
    /// </summary>
    public int GetHorizontalDirection(int playerIndex = 0)
    {
        if (!isTouching) return 0;
        if (Mathf.Abs(smoothHorizontalInput) < 0.15f) return 0;
        return smoothHorizontalInput > 0 ? 1 : -1;
    }

    /// <summary>
    /// 获取水平连续输入（-1.0 ~ 1.0，用于更平滑的移动）
    /// </summary>
    public float GetHorizontalAxis()
    {
        return smoothHorizontalInput;
    }

    public bool GetJumpTrigger(int playerIndex = 0)
    {
        return jumpTriggered;
    }

    public bool GetAttackTrigger(int playerIndex = 0)
    {
        return attackTriggered;
    }

    public bool IsTouching(int playerIndex = 0)
    {
        return isTouching;
    }

    public Vector2 GetTouchPosition(int playerIndex = 0)
    {
        return touchCurrentPosition;
    }

    public Vector2 GetTouchDelta(int playerIndex = 0)
    {
        if (!isTouching) return Vector2.zero;
        return touchCurrentPosition - touchStartPosition;
    }

    // 保留旧接口兼容性
    public bool IsTouchInBottomHalf(Vector2 touchPosition) => touchPosition.y <= Screen.height * 0.5f;
    public bool IsTouchInTopHalf(Vector2 touchPosition) => touchPosition.y > Screen.height * 0.5f;
}
