using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public static class StateEvents
{
    public static event Action<string> OnStateChanged;
    public static void NotifyStateChange(string stateName)
    {
        OnStateChanged?.Invoke(stateName);
    }
}

public class StateWidget : Widget 
{
    [Header("State Display")]
    public TextMeshProUGUI stateText;
    public Canvas canvas;
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float smoothSpeed = 5f;

    private Transform playerTransform;
    private Camera mainCamera;
    private string currentStateName = "None";

    protected override void Start()
    {
        base.Start();
        
    }
    private void SetupReferecne()
    {
        mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
        }

        Controller controller = FindFirstObjectByType<Controller>();
        if(controller != null)
        {
            playerTransform = controller.transform;
        }
    }

    void Update()
    {
        if(playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            if(mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }
    }

    protected override void Subscribe()
    {
        StateEvents.OnStateChanged += HandleStateChange;
    }

    protected override void UnSubscribe()
    {
        StateEvents.OnStateChanged -= HandleStateChange;
    }

    protected virtual void HandleStateChange(string stateName)
    {
        UpdateStateDisplay(stateName);
    }
    private void UpdateStateDisplay(string stateName)
    {
        if(currentStateName != stateName)
        {
            currentStateName = stateName;
            if (stateText != null)
            {
                stateText.text = $"State: {stateName}";
                StartCoroutine(AnimationStateChange());
            }
        }
    }

    private System.Collections.IEnumerator AnimationStateChange()
    {
        Vector3 originalScale = stateText.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.2f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; 
            stateText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            stateText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        stateText.transform.localScale = originalScale;
    }


}