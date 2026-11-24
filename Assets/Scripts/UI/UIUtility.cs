using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtility
{
    // Fait défiler la ScrollRect qui contient 'content' jusqu'en bas,
    // pour que l'élément qui vient d'être ajouté soit visible.
    public static void ScrollToBottom(RectTransform content)
    {
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();

        if (scrollRect == null)
            return;

        // Le layout n'est recalculé qu'en fin de frame : on le force pour que
        // la taille du contenu tienne compte du nouvel élément.
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // Donne le focus au champ (ouvre le clavier sur mobile).
    public static void Focus(TMP_InputField field)
    {
        field.Select();
        field.ActivateInputField();
    }
}
