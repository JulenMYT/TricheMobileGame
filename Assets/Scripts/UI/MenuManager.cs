using System.Collections;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private MenuPanel defaultPanel;
    [SerializeField] private float fadeDuration;

    private MenuPanel currentMenu;

    // Transition en cours et menu en train de se fermer, pour pouvoir
    // interrompre proprement une transition si une autre est demandée.
    private Coroutine switchRoutine;
    private MenuPanel closingMenu;

    private void Awake()
    {
        ServiceLocator.Register<MenuManager>(this);
        currentMenu = defaultPanel;
    }

    private void Start()
    {
        if (defaultPanel != null)
            SwitchTo(defaultPanel);
    }

    public void ToggleMenu(MenuPanel panel)
    {
        SwitchTo(currentMenu == panel ? null : panel);
    }

    public void CloseAll() => SwitchTo(null);

    // Affiche ce menu, sans le refermer s'il est déjà ouvert (contrairement à ToggleMenu).
    public void ShowMenu(MenuPanel panel)
    {
        if (currentMenu != panel)
            SwitchTo(panel);
    }

    private void SwitchTo(MenuPanel panel)
    {
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;

            // Termine immédiatement la fermeture interrompue.
            if (closingMenu != null)
            {
                Hide(closingMenu);

                if (currentMenu == closingMenu)
                    currentMenu = null;

                closingMenu = null;
            }
        }

        switchRoutine = StartCoroutine(SwitchRoutine(panel));
    }

    private IEnumerator SwitchRoutine(MenuPanel panel)
    {
        //Close Old Menu
        if (currentMenu != null && currentMenu != panel)
        {
            closingMenu = currentMenu;

            // Désactive les clics dès le début du fondu pour éviter les doubles appuis.
            closingMenu.canvasGroup.interactable = false;
            closingMenu.canvasGroup.blocksRaycasts = false;

            yield return Fade(closingMenu.canvasGroup, closingMenu.canvasGroup.alpha, 0);

            closingMenu.Close();
            closingMenu = null;
        }

        //Open New Menu
        currentMenu = panel;

        if (currentMenu != null)
        {
            currentMenu.Open();
            CanvasGroup newGroup = currentMenu.canvasGroup;
            newGroup.interactable = true;
            newGroup.blocksRaycasts = true;
            yield return Fade(newGroup, newGroup.alpha, 1);
        }

        switchRoutine = null;
    }

    private void Hide(MenuPanel panel)
    {
        panel.canvasGroup.alpha = 0;
        panel.canvasGroup.interactable = false;
        panel.canvasGroup.blocksRaycasts = false;
        panel.Close();
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
