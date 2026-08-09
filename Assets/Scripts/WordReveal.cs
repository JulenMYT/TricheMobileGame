using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordReveal : MenuPanel
{
    [SerializeField] private TMP_Text categoryText;

    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text wordText;

    [SerializeField] private GameObject clickImage;
    [SerializeField] private GameObject nextButton;

    [SerializeField] private Color legitColour;
    [SerializeField] private Color fakeColour;

    public void Setup(string category, string word, string player, bool fake)
    {
        categoryText.text = category;
        playerName.text = player;

        wordText.text = word;
        wordText.color = fake ? fakeColour : legitColour;

        clickImage.SetActive(true);
        nextButton.SetActive(false);
    }

    public void RevealWord()
    {
        clickImage.SetActive(false);
        nextButton.SetActive(true);
    }
}