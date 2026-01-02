using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private CanvasGroup revealCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private TMP_Text beginText;
    [SerializeField] private TMP_Text fakeText;

    [SerializeField] private Button revealButton;
    [SerializeField] private Button nextButton;

    public event Action OnNextButtonClicked;

    private void Awake()
    {
        revealButton.onClick.AddListener(() => { Reveal(); });
        nextButton.onClick.AddListener(() => OnNextButtonClicked?.Invoke());
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

    public void Setup(string begin, List<string> fakes)
    {
        beginText.text = begin;
        fakeText.text = string.Join(", ", fakes);

        ShowClickImage();
        DisableNextButton();
    }

    public void ShowClickImage()
    {
        revealCanvasGroup.alpha = 1;
        revealCanvasGroup.interactable = true;
        revealCanvasGroup.blocksRaycasts = true;
    }

    public void HideClickImage()
    {
        revealCanvasGroup.alpha = 0;
        revealCanvasGroup.interactable = false;
        revealCanvasGroup.blocksRaycasts = false;
    }

    private void EnableNextButton()
    {
        nextButton.gameObject.SetActive(true);
    }

    private void DisableNextButton()
    {
        nextButton?.gameObject.SetActive(false);
    }

    private void Reveal()
    {
        void Callback()
        {
            HideClickImage();
            EnableNextButton();
        }

        StartCoroutine(FadeCanvasGroup(revealCanvasGroup, 0, fadeDuration, Callback));
        revealCanvasGroup.interactable = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration, Action onComplete = null)
    {
        float startAlpha = group.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        group.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}
