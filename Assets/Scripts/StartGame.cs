using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGame : MenuPanel
{
    [SerializeField] private CanvasGroup revealCanvasGroup;

    [SerializeField] private TMP_Text beginText;
    [SerializeField] private TMP_Text fakeText;
    [SerializeField] private TMP_Text wordText;

    [SerializeField] private GameObject nextButton;

    public void Setup(string begin, List<string> fakes, string word)
    {
        beginText.text = begin;
        fakeText.text = string.Join(", ", fakes);
        wordText.text = word;

        revealCanvasGroup.alpha = 1;
        revealCanvasGroup.interactable = true;
        revealCanvasGroup.blocksRaycasts = true;

        nextButton.SetActive(false);
    }

    public void Reveal()
    {
        StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        revealCanvasGroup.interactable = false;

        yield return FadeCanvasGroup(revealCanvasGroup, 0, 0.5f);

        nextButton.SetActive(true);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            yield return null;
        }

        group.alpha = targetAlpha;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}