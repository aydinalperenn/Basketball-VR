using System.Globalization;
using System.Text;
using UnityEngine;

public class PlayerSessionManager : MonoBehaviour
{
    public static PlayerSessionManager Instance { get; private set; }

    [Header("Player Info")]
    [SerializeField] private string playerName = "UNKNOWN";

    public string PlayerName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return "UNKNOWN";

            return playerName;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            playerName = "UNKNOWN";
        }
        else
        {
            playerName = ConvertToEnglishSafeName(newName);
        }

        Debug.Log("Player name saved: " + playerName);
    }

    public string GetDisplayNameForMenu()
    {
        if (PlayerName == "UNKNOWN")
            return "";

        return PlayerName;
    }

    public string GetSafePlayerNameForFile()
    {
        return ConvertToEnglishSafeName(PlayerName);
    }

    private string ConvertToEnglishSafeName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return "UNKNOWN";

        string text = rawName.Trim();

        // Turkish character conversion
        text = text.Replace("ç", "c").Replace("Ç", "C");
        text = text.Replace("ð", "g").Replace("Ð", "G");
        text = text.Replace("ý", "i").Replace("I", "I");
        text = text.Replace("Ý", "I").Replace("i", "i");
        text = text.Replace("ö", "o").Replace("Ö", "O");
        text = text.Replace("þ", "s").Replace("Þ", "S");
        text = text.Replace("ü", "u").Replace("Ü", "U");

        // Remove remaining accents
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToUpperInvariant(c));
            }
            else if (c == ' ' || c == '_' || c == '-')
            {
                builder.Append('_');
            }
        }

        string cleaned = builder.ToString();

        while (cleaned.Contains("__"))
            cleaned = cleaned.Replace("__", "_");

        cleaned = cleaned.Trim('_');

        if (string.IsNullOrWhiteSpace(cleaned))
            return "UNKNOWN";

        return cleaned;
    }
}