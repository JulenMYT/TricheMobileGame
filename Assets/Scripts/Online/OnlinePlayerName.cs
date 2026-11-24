using TMPro;
using UnityEngine;

// Retient le dernier pseudo utilisé en ligne, pour ne pas le ressaisir à chaque fois.
public static class OnlinePlayerName
{
    private const string Key = "Online_PlayerName";

    public static void Fill(TMP_InputField field)
    {
        if (string.IsNullOrWhiteSpace(field.text))
            field.text = PlayerPrefs.GetString(Key, string.Empty);
    }

    public static void Save(string playerName)
    {
        PlayerPrefs.SetString(Key, playerName);
        PlayerPrefs.Save();
    }
}
