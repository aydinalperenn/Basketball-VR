using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class ShotLogger : MonoBehaviour
{
    private string currentFilePath = "";

    private void Start()
    {
        EnsureLogFile();
        Debug.Log("Active CSV File: " + currentFilePath);
    }

    public void LogShot(
        int shotId,
        float releaseSpeed,
        float releaseAngle,
        bool isScore,
        string shotPointName,
        string firstHitObject,
        string hitRegion,
        string firstHitColliderName,
        Vector3 hitWorldPosition,
        bool hasHitWorldPosition,
        string finalResult)
    {
        EnsureLogFile();

        string playerName = GetCurrentPlayerName();
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        string hitWorldX = hasHitWorldPosition ? hitWorldPosition.x.ToString("F3", CultureInfo.InvariantCulture) : "";
        string hitWorldY = hasHitWorldPosition ? hitWorldPosition.y.ToString("F3", CultureInfo.InvariantCulture) : "";
        string hitWorldZ = hasHitWorldPosition ? hitWorldPosition.z.ToString("F3", CultureInfo.InvariantCulture) : "";

        string line =
            EscapeCsv(playerName) + "," +
            EscapeCsv(shotPointName) + "," +
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

    // Önceki sürümle uyumluluk için.
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
        string shotPointName = GetCurrentShotPointName();

        LogShot(
            shotId,
            releaseSpeed,
            releaseAngle,
            isScore,
            shotPointName,
            firstHitObject,
            hitRegion,
            firstHitColliderName,
            hitWorldPosition,
            hasHitWorldPosition,
            finalResult
        );
    }

    // En eski sürümle uyumluluk için.
    public void LogShot(
        int shotId,
        float releaseSpeed,
        float releaseAngle,
        bool isScore,
        string firstHitObject,
        string finalResult)
    {
        string shotPointName = GetCurrentShotPointName();

        LogShot(
            shotId,
            releaseSpeed,
            releaseAngle,
            isScore,
            shotPointName,
            firstHitObject,
            firstHitObject,
            "",
            Vector3.zero,
            false,
            finalResult
        );
    }

    private void EnsureLogFile()
    {
        if (!string.IsNullOrEmpty(currentFilePath))
            return;

        string fileName = "basket_shot_log.csv";

#if UNITY_EDITOR
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
#else
        string folderPath = Application.persistentDataPath;
#endif

        currentFilePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(currentFilePath))
        {
            string header =
                "PlayerName," +
                "ShotPointName," +
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

            File.WriteAllText(currentFilePath, header + "\n", Encoding.UTF8);
        }
    }

    private string GetCurrentPlayerName()
    {
        if (PlayerSessionManager.Instance == null)
            return "UNKNOWN";

        return PlayerSessionManager.Instance.GetSafePlayerNameForFile();
    }

    private string GetCurrentShotPointName()
    {
        if (ShotPointManager.Instance == null)
            return "UnknownPoint";

        return ShotPointManager.Instance.GetCurrentShotPointName();
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