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
    [SerializeField] private float minSwipeDistance = 50f;

    private Vector2[] touchStartPositions = new Vector2[2];
    private Vector2[] touchCurrentPositions = new Vector2[2];
    private bool[] isTouching = new bool[2];
    private SwipeDirection[] lastSwipeDirections = new SwipeDirection[2];
    private bool[] jumpTriggered = new bool[2];
    private bool[] attackTriggered = new bool[2];

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
        ResetTriggers();

        for (int i = 0; i < 2; i++)
        {
            lastSwipeDirections[i] = SwipeDirection.None;
        }

        HandleTouchInput();
    }

    private void ResetTriggers()
    {
        for (int i = 0; i < 2; i++)
        {
            jumpTriggered[i] = false;
            attackTriggered[i] = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                isTouching[i] = false;
            }
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            int playerIndex = GetPlayerIndexFromTouch(touch.position);

            if (playerIndex < 0 || playerIndex > 1) continue;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPositions[playerIndex] = touch.position;
                    touchCurrentPositions[playerIndex] = touch.position;
                    isTouching[playerIndex] = true;
                    break;

                case TouchPhase.Moved:
                    touchCurrentPositions[playerIndex] = touch.position;
                    DetectSwipe(playerIndex);
                    break;

                case TouchPhase.Ended:
                    DetectSwipe(playerIndex);
                    isTouching[playerIndex] = false;
                    break;

                case TouchPhase.Canceled:
                    isTouching[playerIndex] = false;
                    break;
            }
        }
    }

    private int GetPlayerIndexFromTouch(Vector2 touchPosition)
    {
        if (touchPosition.y <= Screen.height * 0.5f)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    private void DetectSwipe(int playerIndex)
    {
        Vector2 swipeVector = touchCurrentPositions[playerIndex] - touchStartPositions[playerIndex];
        float swipeDistance = swipeVector.magnitude;

        if (swipeDistance >= minSwipeDistance)
        {
            SwipeDirection direction = CalculateSwipeDirection(swipeVector);
            lastSwipeDirections[playerIndex] = direction;

            if (direction == SwipeDirection.Up)
            {
                jumpTriggered[playerIndex] = true;
            }
            else if (direction == SwipeDirection.Down)
            {
                attackTriggered[playerIndex] = true;
            }

            touchStartPositions[playerIndex] = touchCurrentPositions[playerIndex];
        }
    }

    private SwipeDirection CalculateSwipeDirection(Vector2 swipeVector)
    {
        float absX = Mathf.Abs(swipeVector.x);
        float absY = Mathf.Abs(swipeVector.y);

        if (absX > absY)
        {
            return swipeVector.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }
        else
        {
            return swipeVector.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    public SwipeDirection GetSwipeDirection(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return SwipeDirection.None;
        return lastSwipeDirections[playerIndex];
    }

    public int GetHorizontalDirection(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return 0;
        if (!isTouching[playerIndex]) return 0;

        float deltaX = touchCurrentPositions[playerIndex].x - touchStartPositions[playerIndex].x;

        if (Mathf.Abs(deltaX) < minSwipeDistance * 0.5f)
        {
            return 0;
        }

        return deltaX > 0 ? 1 : -1;
    }

    public bool GetJumpTrigger(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return false;
        return jumpTriggered[playerIndex];
    }

    public bool GetAttackTrigger(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return false;
        return attackTriggered[playerIndex];
    }

    public bool IsTouching(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return false;
        return isTouching[playerIndex];
    }

    public bool IsTouchInBottomHalf(Vector2 touchPosition)
    {
        return touchPosition.y <= Screen.height * 0.5f;
    }

    public bool IsTouchInTopHalf(Vector2 touchPosition)
    {
        return touchPosition.y > Screen.height * 0.5f;
    }

    public Vector2 GetTouchPosition(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return Vector2.zero;
        return touchCurrentPositions[playerIndex];
    }

    public Vector2 GetTouchDelta(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return Vector2.zero;
        if (!isTouching[playerIndex]) return Vector2.zero;
        return touchCurrentPositions[playerIndex] - touchStartPositions[playerIndex];
    }
}
