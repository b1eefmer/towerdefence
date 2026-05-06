using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager main;

    private readonly List<RectTransform> blockingUiRects = new();
    private static readonly string[] BlockingUiObjectNames =
    {
        "Menu",
        "Menu Toggle",
        "PauseGameButton",
        "Start"
    };

    private bool isHoveringUI;
    public void Awake ()
    {
        main = this;
    }
    public void SetHoveringState (bool state)
    {
        isHoveringUI = state;
    }
    public bool IsHoveringUI()
    {
        return isHoveringUI;
    }

    public bool IsPointerOverBlockingUI()
    {
        Vector2 screenPosition = Input.mousePosition;
        CacheBlockingUiRects();

        foreach (RectTransform rectTransform in blockingUiRects)
        {
            if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
            {
                continue;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, eventCamera))
            {
                return true;
            }
        }

        UpgradeUIHandler[] upgradeUiHandlers = FindObjectsByType<UpgradeUIHandler>(FindObjectsSortMode.None);
        foreach (UpgradeUIHandler upgradeUiHandler in upgradeUiHandlers)
        {
            if (upgradeUiHandler == null || !upgradeUiHandler.gameObject.activeInHierarchy)
            {
                continue;
            }

            RectTransform upgradeRect = upgradeUiHandler.transform as RectTransform;
            if (upgradeRect == null)
            {
                continue;
            }

            Canvas canvas = upgradeUiHandler.GetComponentInParent<Canvas>();
            Camera eventCamera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                eventCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(upgradeRect, screenPosition, eventCamera))
            {
                return true;
            }
        }

        return false;
    }

    private void CacheBlockingUiRects()
    {
        blockingUiRects.RemoveAll(rect => rect == null);
        if (blockingUiRects.Count >= BlockingUiObjectNames.Length)
        {
            return;
        }

        foreach (string objectName in BlockingUiObjectNames)
        {
            if (ContainsBlockingRect(objectName))
            {
                continue;
            }

            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                continue;
            }

            RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                blockingUiRects.Add(rectTransform);
            }
        }
    }

    private bool ContainsBlockingRect(string objectName)
    {
        foreach (RectTransform rectTransform in blockingUiRects)
        {
            if (rectTransform != null && rectTransform.name == objectName)
            {
                return true;
            }
        }

        return false;
    }
}
