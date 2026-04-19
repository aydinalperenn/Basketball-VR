using System.Globalization;
using System.IO;
using UnityEngine;

public class ShotLogger : MonoBehaviour
{
    private string filePath;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "basket_shot_log.csv");
        CreateFileIfNeeded();

        Debug.Log("Shot log dosya yolu: " + filePath);
    }

    private void CreateFileIfNeeded()
    {
        if (File.Exists(filePath))
            return;

        string header = "ShotId,Time,ReleaseSpeed,ReleaseAngle,Scored,FirstHitObject,FinalResult\n";
        File.WriteAllText(filePath, header);
    }

    public void LogShot(int shotId, float releaseSpeed, float releaseAngle, bool scored, string firstHitObject, string finalResult)
    {
        string speedText = releaseSpeed.ToString("F2", CultureInfo.InvariantCulture);
        string angleText = releaseAngle.ToString("F2", CultureInfo.InvariantCulture);

        string line =
            shotId + "," +
            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," +
            speedText + "," +
            angleText + "," +
            scored + "," +
            firstHitObject + "," +
            finalResult + "\n";

        File.AppendAllText(filePath, line);

        Debug.Log("Atış kaydedildi: " + line);
    }

    public string GetFilePath()
    {
        return filePath;
    }
}