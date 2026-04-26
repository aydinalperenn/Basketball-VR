using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class ShotLogger : MonoBehaviour
{
    private string currentFilePath = "";
    private string activePlayerName = "";

    public void LogShot(
        int shotId,
        float releaseSpeed,
        float releaseAngle,
        bool isScore,
        string firstHitObject,
        string finalResult)
    {
        string playerName = GetCurrentPlayerName();
        EnsureLogFile(playerName);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        string englishHitObject = ConvertHitObjectToEnglish(firstHitObject);
        string englishFinalResult = isScore ? "Score" : "Miss";

        string line =
            EscapeCsv(playerName) + "," +
            shotId.ToString(CultureInfo.InvariantCulture) + "," +
            EscapeCsv(timestamp) + "," +
            releaseSpeed.ToString("F3", CultureInfo.InvariantCulture) + "," +
            releaseAngle.ToString("F3", CultureInfo.InvariantCulture) + "," +
            isScore.ToString(CultureInfo.InvariantCulture) + "," +
            EscapeCsv(englishHitObject) + "," +
            EscapeCsv(englishFinalResult);

        File.AppendAllText(currentFilePath, line + "\n", Encoding.UTF8);

        Debug.Log("Shot logged: " + currentFilePath);
    }

    private void EnsureLogFile(string playerName)
    {
        if (!string.IsNullOrEmpty(currentFilePath) && activePlayerName == playerName)
            return;

        activePlayerName = playerName;

        string sessionTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string fileName = "basket_shot_log_" + playerName + "_" + sessionTime + ".csv";

        currentFilePath = Path.Combine(Application.persistentDataPath, fileName);

        string header =
            "PlayerName," +
            "ShotId," +
            "Timestamp," +
            "ReleaseSpeedMS," +
            "ReleaseAngleDeg," +
            "IsScore," +
            "FirstHitObject," +
            "FinalResult";

        File.WriteAllText(currentFilePath, header + "\n", Encoding.UTF8);

        Debug.Log("New shot log file created: " + currentFilePath);
    }

    private string GetCurrentPlayerName()
    {
        if (PlayerSessionManager.Instance == null)
            return "UNKNOWN";

        return PlayerSessionManager.Instance.GetSafePlayerNameForFile();
    }

    private string ConvertHitObjectToEnglish(string hitObject)
    {
        if (string.IsNullOrWhiteSpace(hitObject))
            return "None";

        string value = hitObject.Trim();

        // Turkish values from older scripts
        if (value == "Pota Çemberi")
            return "Rim";

        if (value == "Pota Cemberi")
            return "Rim";

        if (value == "Panya")
            return "Backboard";

        if (value == "Zemin")
            return "Floor";

        if (value == "Temassız")
            return "NoContact";

        if (value == "Temassiz")
            return "NoContact";

        if (value == "Yok")
            return "None";

        // English values already
        if (value == "Rim")
            return "Rim";

        if (value == "Backboard")
            return "Backboard";

        if (value == "Floor")
            return "Floor";

        if (value == "NoContact")
            return "NoContact";

        return ConvertToEnglishAscii(value);
    }

    private string ConvertToEnglishAscii(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "None";

        string text = input.Trim();

        text = text.Replace("ç", "c").Replace("Ç", "C");
        text = text.Replace("ğ", "g").Replace("Ğ", "G");
        text = text.Replace("ı", "i").Replace("İ", "I");
        text = text.Replace("ö", "o").Replace("Ö", "O");
        text = text.Replace("ş", "s").Replace("Ş", "S");
        text = text.Replace("ü", "u").Replace("Ü", "U");

        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (c <= 127)
                builder.Append(c);
        }

        return builder.ToString();
    }

    private string EscapeCsv(string value)
    {
        if (value == null)
            return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            return "\"" + value + "\"";
        }

        return value;
    }
}