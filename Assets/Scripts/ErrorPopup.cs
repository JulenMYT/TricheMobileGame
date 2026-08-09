using TMPro;
using UnityEngine;

public class ErrorPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float speed = 30f;

    private float timer;

    public void Setup(string message)
    {
        popupText.text = message;
    }

    private void Update()
    {
        popupText.rectTransform.position += Vector3.up * (speed * Time.deltaTime);

        timer += Time.deltaTime;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}