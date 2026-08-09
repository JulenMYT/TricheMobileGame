using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private MenuPanel defaultPanel;
    [SerializeField] private float fadeDuration;

    private MenuPanel currentMenu;

    private void Awake()
    {
        ServiceLocator.Register<MenuManager>(this);
        currentMenu = defaultPanel;
    }

    private void Start()
    {
        if (defaultPanel != null)
            StartCoroutine(SwitchRoutine(defaultPanel));
    }

    public void ToggleMenu(MenuPanel panel)
    {
        if (currentMenu == panel)
        {
            StartCoroutine(SwitchRoutine(null));
            return;
        }

        StartCoroutine(SwitchRoutine(panel));
    }

    public void CloseAll() => StartCoroutine(SwitchRoutine(null));

    private IEnumerator SwitchRoutine(MenuPanel panel)
    {
        //Close Old Menu
        if (currentMenu != null)
        {
            yield return Fade(currentMenu.canvasGroup, currentMenu.canvasGroup.alpha, 0);
            currentMenu.canvasGroup.interactable = false;
            currentMenu.canvasGroup.blocksRaycasts = false;
            currentMenu.Close();
        }

        //Open New Menu
        currentMenu = panel;

        if (currentMenu != null)
        {
            currentMenu.Open();
            CanvasGroup newGroup = currentMenu.canvasGroup;
            currentMenu.canvasGroup.interactable = true;
            currentMenu.canvasGroup.blocksRaycasts = true;
            yield return Fade(newGroup, newGroup.alpha, 1);
        }
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float from, float to)
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
