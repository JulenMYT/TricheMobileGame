using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Adapte le seuil de glissement de l'EventSystem à la densité de l'écran.
//
// Par défaut, il suffit de 10 pixels pour qu'un appui devienne un glissement.
// Sur un téléphone à haute densité, c'est à peine 1 mm : un simple tap sur un
// bouton d'une liste est souvent pris pour un défilement et le clic est perdu.
public static class DragThresholdScaler
{
    // Environ 10 px sur un écran de référence à 160 dpi, soit ~1,6 mm.
    private const float ReferenceDpi = 160f;
    private const int ReferenceThreshold = 10;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        Apply();
        SceneManager.sceneLoaded += (_, _) => Apply();
    }

    private static void Apply()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
            eventSystem = Object.FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
            return;

        float dpi = Screen.dpi > 0 ? Screen.dpi : ReferenceDpi;
        int threshold = Mathf.RoundToInt(ReferenceThreshold * dpi / ReferenceDpi);

        eventSystem.pixelDragThreshold = Mathf.Max(ReferenceThreshold, threshold);
    }
}
