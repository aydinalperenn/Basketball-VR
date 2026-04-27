using TMPro;
using UnityEngine;

public class ShotResultUI : MonoBehaviour
{
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text angleText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text hitText;

    public void ShowShotResult(
        float speed,
        float angle,
        bool isScore,
        string firstHitObject,
        string hitRegion)
    {
        if (speedText != null)
            speedText.text = "Top Hızı: " + speed.ToString("F2") + " m/s";

        if (angleText != null)
            angleText.text = "Atış Açısı: " + angle.ToString("F2") + "°";

        if (resultText != null)
            resultText.text = isScore ? "Sonuç: SAYI" : "Sonuç: KAÇTI";

        string readableHitObject = firstHitObject == "Yok" ? "Temassız" : firstHitObject;
        string readableRegion = hitRegion == "Yok" ? readableHitObject : hitRegion;

        if (hitText != null)
        {
            if (isScore)
                hitText.text = "İlk Temas: " + readableRegion;
            else
                hitText.text = "Çarptığı Yer: " + readableRegion;
        }
    }

    public void ShowShotResult(float speed, float angle, bool isScore, string firstHitObject)
    {
        ShowShotResult(speed, angle, isScore, firstHitObject, firstHitObject);
    }

    public void ClearUI()
    {
        if (speedText != null)
            speedText.text = "Top Hızı: -";

        if (angleText != null)
            angleText.text = "Atış Açısı: -";

        if (resultText != null)
            resultText.text = "Sonuç: -";

        if (hitText != null)
            hitText.text = "Çarptığı Yer: -";
    }
}