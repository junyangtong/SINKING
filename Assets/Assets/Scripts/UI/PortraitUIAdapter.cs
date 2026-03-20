using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PortraitUIAdapter : MonoBehaviour
{
    [Header("Portrait Settings")]
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    [Header("Split Screen Settings")]
    [SerializeField] private float splitScreenTopOffset = 0.5f;
    [SerializeField] private float splitScreenBottomOffset = 0.5f;
    [SerializeField] private bool autoDetectSplitScreen = true;

    [Header("UI Elements to Adapt")]
    [SerializeField] private RectTransform[] uiElementsToAdapt;
    [SerializeField] private Vector2[] portraitPositions;

    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private bool isSplitScreenMode = false;
    private Vector2[] originalPositions;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        if (canvasScaler == null)
        {
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
        }

        StoreOriginalPositions();
    }

    private void Start()
    {
        AdaptToPortrait();

        if (autoDetectSplitScreen && SplitScreenManager.Instance != null)
        {
            isSplitScreenMode = SplitScreenManager.Instance.isSplitScreenMode;
        }
    }

    private void Update()
    {
        if (autoDetectSplitScreen && SplitScreenManager.Instance != null)
        {
            if (SplitScreenManager.Instance.isSplitScreenMode != isSplitScreenMode)
            {
                SetSplitScreenUI(SplitScreenManager.Instance.isSplitScreenMode);
            }
        }
    }

    public void AdaptToPortrait()
    {
        if (canvasScaler == null)
        {
            Debug.LogWarning("PortraitUIAdapter: CanvasScaler not found.");
            return;
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = portraitReferenceResolution;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;

        ApplyPortraitPositions();

        Debug.Log($"PortraitUIAdapter: Adapted to portrait mode. Reference Resolution: {portraitReferenceResolution}");
    }

    public void SetSplitScreenUI(bool isSplitScreen)
    {
        isSplitScreenMode = isSplitScreen;

        if (uiElementsToAdapt == null || uiElementsToAdapt.Length == 0)
        {
            return;
        }

        for (int i = 0; i < uiElementsToAdapt.Length; i++)
        {
            if (uiElementsToAdapt[i] == null) continue;

            if (isSplitScreen)
            {
                AdaptElementForSplitScreen(uiElementsToAdapt[i], i);
            }
            else
            {
                AdaptElementForFullScreen(uiElementsToAdapt[i], i);
            }
        }

        Debug.Log($"PortraitUIAdapter: Split screen mode set to {isSplitScreen}");
    }

    public void ConvertHorizontalToPortraitPosition(RectTransform element, Vector2 horizontalPos, Vector2 verticalPos)
    {
        if (element == null) return;

        float aspectRatio = (float)Screen.width / Screen.height;
        bool isPortrait = aspectRatio < 1f;

        if (isPortrait)
        {
            element.anchoredPosition = verticalPos;
        }
        else
        {
            element.anchoredPosition = horizontalPos;
        }
    }

    public void AdaptElementPosition(RectTransform element, Vector2 targetPosition)
    {
        if (element == null)
        {
            Debug.LogWarning("PortraitUIAdapter: Cannot adapt null element.");
            return;
        }

        element.anchoredPosition = targetPosition;
    }

    public void AdaptElementSize(RectTransform element, Vector2 size)
    {
        if (element == null)
        {
            Debug.LogWarning("PortraitUIAdapter: Cannot adapt size of null element.");
            return;
        }

        element.sizeDelta = size;
    }

    public void SetElementAnchor(RectTransform element, Vector2 minAnchor, Vector2 maxAnchor)
    {
        if (element == null) return;

        element.anchorMin = minAnchor;
        element.anchorMax = maxAnchor;
    }

    public void AdaptElementForTopScreen(RectTransform element)
    {
        if (element == null) return;

        Vector2 currentPos = element.anchoredPosition;
        element.anchoredPosition = new Vector2(currentPos.x, currentPos.y + (portraitReferenceResolution.y * splitScreenTopOffset));
    }

    public void AdaptElementForBottomScreen(RectTransform element)
    {
        if (element == null) return;

        Vector2 currentPos = element.anchoredPosition;
        element.anchoredPosition = new Vector2(currentPos.x, currentPos.y - (portraitReferenceResolution.y * splitScreenBottomOffset));
    }

    public bool IsPortraitMode()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        return aspectRatio < 1f;
    }

    public float GetCurrentAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }

    public Vector2 GetSafeAreaOffset()
    {
        Rect safeArea = Screen.safeArea;
        float offsetX = safeArea.x;
        float offsetY = safeArea.y;

        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            float scaleFactor = canvasScaler.referenceResolution.x / Screen.width;
            offsetX *= scaleFactor;
            offsetY *= scaleFactor;
        }

        return new Vector2(offsetX, offsetY);
    }

    public void ApplySafeAreaPadding(RectTransform element)
    {
        if (element == null) return;

        Vector2 safeAreaOffset = GetSafeAreaOffset();
        Vector2 currentOffset = element.offsetMin;
        element.offsetMin = new Vector2(currentOffset.x + safeAreaOffset.x, currentOffset.y + safeAreaOffset.y);
    }

    private void StoreOriginalPositions()
    {
        if (uiElementsToAdapt == null || uiElementsToAdapt.Length == 0)
        {
            return;
        }

        originalPositions = new Vector2[uiElementsToAdapt.Length];
        for (int i = 0; i < uiElementsToAdapt.Length; i++)
        {
            if (uiElementsToAdapt[i] != null)
            {
                originalPositions[i] = uiElementsToAdapt[i].anchoredPosition;
            }
        }
    }

    private void ApplyPortraitPositions()
    {
        if (uiElementsToAdapt == null || portraitPositions == null)
        {
            return;
        }

        int count = Mathf.Min(uiElementsToAdapt.Length, portraitPositions.Length);
        for (int i = 0; i < count; i++)
        {
            if (uiElementsToAdapt[i] != null)
            {
                uiElementsToAdapt[i].anchoredPosition = portraitPositions[i];
            }
        }
    }

    private void AdaptElementForSplitScreen(RectTransform element, int index)
    {
        if (element == null) return;

        float halfHeight = portraitReferenceResolution.y * 0.5f;
        Vector2 currentPos = element.anchoredPosition;

        if (currentPos.y > 0)
        {
            element.anchoredPosition = new Vector2(currentPos.x, currentPos.y - halfHeight * 0.25f);
        }
        else
        {
            element.anchoredPosition = new Vector2(currentPos.x, currentPos.y + halfHeight * 0.25f);
        }

        float scale = 0.8f;
        element.localScale = new Vector3(scale, scale, 1f);
    }

    private void AdaptElementForFullScreen(RectTransform element, int index)
    {
        if (element == null) return;

        if (originalPositions != null && index < originalPositions.Length)
        {
            element.anchoredPosition = originalPositions[index];
        }

        element.localScale = Vector3.one;
    }

    public void SetReferenceResolution(Vector2 resolution)
    {
        portraitReferenceResolution = resolution;
        if (canvasScaler != null)
        {
            canvasScaler.referenceResolution = resolution;
        }
    }

    public void SetMatchValue(float match)
    {
        matchWidthOrHeight = Mathf.Clamp01(match);
        if (canvasScaler != null)
        {
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }
    }

    public void AddUIElementToAdapt(RectTransform element, Vector2 portraitPosition)
    {
        if (element == null) return;

        System.Collections.Generic.List<RectTransform> elementsList = new System.Collections.Generic.List<RectTransform>();
        System.Collections.Generic.List<Vector2> positionsList = new System.Collections.Generic.List<Vector2>();

        if (uiElementsToAdapt != null)
        {
            elementsList.AddRange(uiElementsToAdapt);
        }
        if (portraitPositions != null)
        {
            positionsList.AddRange(portraitPositions);
        }

        elementsList.Add(element);
        positionsList.Add(portraitPosition);

        uiElementsToAdapt = elementsList.ToArray();
        portraitPositions = positionsList.ToArray();

        StoreOriginalPositions();
    }

    public void RemoveUIElementFromAdapt(RectTransform element)
    {
        if (element == null || uiElementsToAdapt == null) return;

        System.Collections.Generic.List<RectTransform> elementsList = new System.Collections.Generic.List<RectTransform>(uiElementsToAdapt);
        System.Collections.Generic.List<Vector2> positionsList = portraitPositions != null ? 
            new System.Collections.Generic.List<Vector2>(portraitPositions) : new System.Collections.Generic.List<Vector2>();

        int index = elementsList.IndexOf(element);
        if (index >= 0)
        {
            elementsList.RemoveAt(index);
            if (positionsList.Count > index)
            {
                positionsList.RemoveAt(index);
            }
        }

        uiElementsToAdapt = elementsList.ToArray();
        portraitPositions = positionsList.ToArray();

        StoreOriginalPositions();
    }
}
