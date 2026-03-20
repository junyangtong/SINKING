using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }

    [Header("Game Mode")]
    public bool isTwoPlayerMode = false;

    [Header("Player References")]
    public Transform player1;
    public Transform player2;

    [Header("Camera References")]
    public Camera mainCamera;
    public Camera player2Camera;

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
            return;
        }

        EnsureSwipeInputManagerExists();
        EnsureSplitScreenManagerExists();
    }

    private void Start()
    {
        SetupCameras();
    }

    private void EnsureSwipeInputManagerExists()
    {
        if (SwipeInputManager.Instance == null)
        {
            GameObject swipeInputManagerObj = new GameObject("SwipeInputManager");
            swipeInputManagerObj.AddComponent<SwipeInputManager>();
            Debug.Log("GameInitializer: SwipeInputManager created automatically.");
        }
    }

    private void EnsureSplitScreenManagerExists()
    {
        if (SplitScreenManager.Instance == null)
        {
            GameObject splitScreenManagerObj = new GameObject("SplitScreenManager");
            splitScreenManagerObj.AddComponent<SplitScreenManager>();
            Debug.Log("GameInitializer: SplitScreenManager created automatically.");
        }
    }

    private void SetupCameras()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "SingleGameScene")
        {
            SetupSinglePlayerMode();
        }
        else if (currentScene == "GameScene")
        {
            SetupTwoPlayerMode();
        }
        else
        {
            Debug.Log($"GameInitializer: Scene '{currentScene}' does not require camera setup.");
        }
    }

    private void SetupSinglePlayerMode()
    {
        if (mainCamera != null)
        {
            Follow cameraFollow = mainCamera.GetComponent<Follow>();
            if (cameraFollow != null && player1 != null)
            {
                cameraFollow.target = player1;
            }
        }

        if (SplitScreenManager.Instance != null)
        {
            SplitScreenManager.Instance.SetupSingleScreen(mainCamera);
        }

        Debug.Log("GameInitializer: Single player mode initialized (SingleGameScene).");
    }

    private void SetupTwoPlayerMode()
    {
        isTwoPlayerMode = true;

        if (SplitScreenManager.Instance != null && mainCamera != null && player2Camera != null)
        {
            SplitScreenManager.Instance.SetupSplitScreen(mainCamera, player2Camera);
        }

        if (mainCamera != null)
        {
            Follow cameraFollow = mainCamera.GetComponent<Follow>();
            if (cameraFollow != null && player1 != null)
            {
                cameraFollow.target = player1;
            }
        }

        if (player2Camera != null)
        {
            Follow player2CameraFollow = player2Camera.GetComponent<Follow>();
            if (player2CameraFollow != null && player2 != null)
            {
                player2CameraFollow.target = player2;
            }
        }

        Debug.Log("GameInitializer: Two player mode initialized (GameScene).");
    }

    public void SetTwoPlayerMode(bool enabled)
    {
        isTwoPlayerMode = enabled;
        SetupCameras();
        Debug.Log($"GameInitializer: Two player mode set to {enabled}");
    }
}
