using UnityEngine;

public class SplitScreenManager : MonoBehaviour
{
    private static SplitScreenManager _instance;
    public static SplitScreenManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SplitScreenManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SplitScreenManager");
                    _instance = go.AddComponent<SplitScreenManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public bool isSplitScreenMode { get; private set; } = false;
    public Camera player1Camera { get; private set; }
    public Camera player2Camera { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetupSplitScreen(Camera camera1, Camera camera2)
    {
        if (camera1 == null || camera2 == null)
        {
            Debug.LogWarning("SplitScreenManager: Cannot setup split screen with null cameras.");
            return;
        }

        player1Camera = camera1;
        player2Camera = camera2;

        camera1.rect = new Rect(0, 0, 1, 0.5f);
        camera2.rect = new Rect(0, 0.5f, 1, 0.5f);

        isSplitScreenMode = true;
        Debug.Log("SplitScreenManager: Split screen mode enabled. Player1 (bottom), Player2 (top).");
    }

    public void SetupSingleScreen(Camera camera)
    {
        if (camera == null)
        {
            Debug.LogWarning("SplitScreenManager: Cannot setup single screen with null camera.");
            return;
        }

        if (player1Camera != null && player1Camera != camera)
        {
            player1Camera.rect = new Rect(0, 0, 1, 1);
        }
        if (player2Camera != null && player2Camera != camera)
        {
            player2Camera.rect = new Rect(0, 0, 1, 1);
        }

        camera.rect = new Rect(0, 0, 1, 1);

        player1Camera = null;
        player2Camera = null;
        isSplitScreenMode = false;
        Debug.Log("SplitScreenManager: Single screen mode enabled.");
    }

    public void DisableSplitScreen()
    {
        if (!isSplitScreenMode)
        {
            return;
        }

        if (player1Camera != null)
        {
            player1Camera.rect = new Rect(0, 0, 1, 1);
        }
        if (player2Camera != null)
        {
            player2Camera.rect = new Rect(0, 0, 1, 1);
        }

        player1Camera = null;
        player2Camera = null;
        isSplitScreenMode = false;
        Debug.Log("SplitScreenManager: Split screen mode disabled.");
    }
}
