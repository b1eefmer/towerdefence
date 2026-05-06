using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Zoom In (current level)")]
    public Transform levelCenter;        
    public float startSize = 10f;        
    public float zoomInSize = 2f;        
    public float zoomInDuration = 0.8f;

    [Header("Flash")]
    public UnityEngine.UI.Image flashPanel;
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;

    [Header("Zoom Out (next level)")]
    public Transform nextLevelCenter;    
    public float zoomOutSize = 10f;      
    public float zoomOutDuration = 0.8f;

    [Header("Scene loading")]
    public LevelManager levelManager;   

  
    public void StartTeleport(string nextSceneName)
    {
        StartCoroutine(TeleportSequence(nextSceneName));
    }

    private IEnumerator TeleportSequence(string nextScene)
    {
       
        Vector3 startPos = targetCamera.transform.position;
        Vector3 targetPosIn = new Vector3(levelCenter.position.x, levelCenter.position.y, startPos.z);
        yield return StartCoroutine(ZoomAndMove(startPos, targetPosIn, startSize, zoomInSize, zoomInDuration));

       
        yield return StartCoroutine(FlashEffect());

        
        Vector3 targetPosOut = new Vector3(nextLevelCenter.position.x, nextLevelCenter.position.y, startPos.z);
        yield return StartCoroutine(ZoomAndMove(targetCamera.transform.position, targetPosOut, zoomInSize, zoomOutSize, zoomOutDuration));

        
        if (levelManager != null)
            levelManager.LoadSceneDirectly(nextScene); 
        else
            SceneManager.LoadScene(nextScene);
    }

    private IEnumerator ZoomAndMove(Vector3 fromPos, Vector3 toPos, float fromZoom, float toZoom, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            targetCamera.transform.position = Vector3.Lerp(fromPos, toPos, t);
            if (targetCamera.orthographic)
                targetCamera.orthographicSize = Mathf.Lerp(fromZoom, toZoom, t);
            else
                targetCamera.fieldOfView = Mathf.Lerp(fromZoom, toZoom, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetCamera.transform.position = toPos;
        if (targetCamera.orthographic) targetCamera.orthographicSize = toZoom;
        else targetCamera.fieldOfView = toZoom;
    }

    private IEnumerator FlashEffect()
    {
        if (flashPanel == null) yield break;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / flashDuration);
            flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f);
        yield return new WaitForSeconds(0.05f);
        flashPanel.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }
}