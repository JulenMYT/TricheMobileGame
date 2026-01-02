using System;
using UnityEngine;
using System.Collections;

public class Transition : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    public event Action OnFadeOutComplete;
    public event Action OnFadeInComplete;

    private void Reset()
    {
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeIn()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        StartCoroutine(FadeCanvas(0f, 1f, fadeDuration, () =>
        {
            OnFadeInComplete?.Invoke();
        }));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeCanvas(canvasGroup.alpha, 0f, fadeDuration, () =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            OnFadeOutComplete?.Invoke();
        }));
    }

    public void FadeInOut()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartCoroutine(FadeCanvas(0f, 1f, fadeDuration, () =>
        {
            OnFadeInComplete?.Invoke();
            StartCoroutine(FadeCanvas(1f, 0f, fadeDuration, () =>
            {
                OnFadeOutComplete?.Invoke();
            }));
        }));
    }

    private IEnumerator FadeCanvas(float from, float to, float duration, Action onComplete)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
        onComplete?.Invoke();
    }
}
