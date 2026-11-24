using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Champ de texte qui laisse défiler la ScrollRect parente.
//
// Un TMP_InputField classique intercepte le glissement (pour sélectionner du texte)
// et la molette : posé dans une liste déroulante, il empêche de la faire défiler.
// Il s'active aussi dès l'appui, ce qui ouvre le clavier sur mobile alors que
// l'utilisateur voulait seulement faire défiler.
//
// Ici :
// - un glissement sur un champ non actif (ou vertical sur un champ d'une ligne)
//   est transmis à la ScrollRect parente ;
// - la molette est transmise à la ScrollRect parente pour les champs d'une ligne ;
// - le champ ne s'active qu'au relâchement, si l'utilisateur n'a pas fait défiler.
public class ScrollFriendlyInputField : TMP_InputField
{
    private ScrollRect parentScroll;
    private bool forwardingDrag;
    private bool pendingActivation;

    private ScrollRect GetParentScroll()
    {
        if (parentScroll == null && transform.parent != null)
            parentScroll = transform.parent.GetComponentInParent<ScrollRect>();

        return parentScroll;
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        parentScroll = null;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        forwardingDrag = false;

        // Déjà actif : comportement normal (placement du curseur, sélection...).
        if (isFocused || GetParentScroll() == null)
        {
            pendingActivation = false;
            base.OnPointerDown(eventData);
            return;
        }

        // Pas encore actif : on attend le relâchement pour savoir si c'est un tap.
        pendingActivation = true;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (forwardingDrag)
            return;

        if (pendingActivation)
        {
            pendingActivation = false;
            base.OnPointerDown(eventData);
        }

        base.OnPointerClick(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (ShouldForwardDrag(eventData))
        {
            forwardingDrag = true;
            pendingActivation = false;
            parentScroll.OnBeginDrag(eventData);
            return;
        }

        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (forwardingDrag)
        {
            parentScroll.OnDrag(eventData);
            return;
        }

        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (forwardingDrag)
        {
            parentScroll.OnEndDrag(eventData);
            return;
        }

        base.OnEndDrag(eventData);
    }

    public override void OnScroll(PointerEventData eventData)
    {
        if (lineType == LineType.SingleLine && GetParentScroll() != null)
        {
            parentScroll.OnScroll(eventData);
            return;
        }

        base.OnScroll(eventData);
    }

    private bool ShouldForwardDrag(PointerEventData eventData)
    {
        if (GetParentScroll() == null)
            return false;

        if (!isFocused)
            return true;

        // Champ actif d'une seule ligne : un glissement vertical fait défiler,
        // un glissement horizontal sélectionne le texte.
        Vector2 delta = eventData.pressPosition - eventData.position;
        return lineType == LineType.SingleLine && Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
    }
}
