using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public virtual void Open() { }
    public virtual void Close() { }
}
