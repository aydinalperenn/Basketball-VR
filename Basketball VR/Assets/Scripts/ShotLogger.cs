using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class ShotLogger : MonoBehaviour
{
    private string currentFilePath = "";
    private string activePlayerName = "";
    private string sessionTime = "";

    private void Start()
    {
        EnsureLogFile(GetCurrentPlayerName());
        Debug.Log("CSV Path: " + currentFilePath);
    }

    public void LogShot(
        int shotId,
        float releaseSpeed,
        float releaseAngle,
        bool isScore,
        string firstHitObject,
        string hitRegion,
        string firstHitColliderName,
        Vector3 hitWorldPosition,
        bool hasHitWorldPosition,
        string finalResult)
    {
        string playerName = GetCurrentPlayerName();
        EnsureLogFile(playerName);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        string hitWorldX = hasHitWorldPosition ? hitWorldPosition.x.ToString("F3", CultureInfo.InvariantCulture) : "";
        string hitWorldY = hasHitWorldPosition ? hitWorldPosition.y.ToString("F3", CultureInfo.InvariantCulture) : "";
        string hitWorldZ = hasHitWorldPosition ? hitWorldPosition.z.ToString("F3", CultureInfo.InvariantCulture) : "";

        string line =
            EscapeCsv(playerName) + "," +
            shotId.ToString(CultureInfo.InvariantCulture) + "," +
            EscapeCsv(timestamp) + "," +
            releaseSpeed.ToString("F3", CultureInfo.InvariantCulture) + "," +
            releaseAngle.ToString("F3", CultureInfo.InvariantCulture) + "," +
            isScore.ToString(CultureInfo.InvariantCulture) + "," +
            EscapeCsv(firstHitObject) + "," +
            EscapeCsv(hitRegion) + "," +
            EscapeCsv(firstHitColliderName) + "," +
            hitWorldX + "," +
            hitWorldY + "," +
            hitWorldZ + "," +
            EscapeCsv(finalResult);

        File.AppendAllText(currentFilePath, line + "\n", Encoding.UTF8);
        Debug.Log("Shot logged: " + currentFilePath);
    }

    private void EnsureLogFile(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "UNKNOWN";

        if (!string.IsNullOrEmpty(currentFilePath) && activePlayerName == playerName)
            return;

        activePlayerName = playerName;

        if (string.IsNullOrEmpty(sessionTime))
            sessionTime = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        string fileName = "basket_shot_log_" + playerName + "_" + sessionTime + ".csv";
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        currentFilePath = Path.Combine(desktopPath, fileName);

        string header =
            "PlayerName," +
            "ShotId," +
            "Timestamp," +
            "ReleaseSpeedMS," +
            "ReleaseAngleDeg," +
            "IsScore," +
            "FirstHitObject," +
            "HitRegion," +
            "FirstHitColliderName," +
            "HitWorldX," +
            "HitWorldY," +
            "HitWorldZ," +
            "FinalResult";

        if (!File.Exists(currentFilePath))
        {
            File.WriteAllText(currentFilePath, header + "\n", Encoding.UTF8);
        }

        Debug.Log("Active CSV File: " + currentFilePath);
    }

    private string GetCurrentPlayerName()
    {
        if (PlayerSessionManager.Instance == null)
            return "UNKNOWN";

        return PlayerSessionManager.Instance.GetSafePlayerNameForFile();
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