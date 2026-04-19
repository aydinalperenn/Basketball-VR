using TMPro;
using UnityEngine;

public class ShotResultUI : MonoBehaviour
{
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text angleText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text hitText;

    public void ShowShotResult(float speed, float angle, bool isScore, string firstHitObject)
    {
        speedText.text = "Top Hızı: " + speed.ToString("F2") + " m/s";
        angleText.text = "Atış Açısı: " + angle.ToString("F2") + "°";
        resultText.text = isScore ? "Sonuç: SAYI" : "Sonuç: KAÇTI";

        string readableHit = firstHitObject == "Yok" ? "Temassız" : firstHitObject;

        if (isScore)
            hitText.text = "İlk Temas: " + readableHit;
        else
            hitText.text = "Çarptığı Yer: " + readableHit;
    }

    public void ClearUI()
    {
        speedText.text = "Top Hızı: -";
        angleText.text = "Atış Açısı: -";
        resultText.text = "Sonuç: -";
        hitText.text = "Çarptığı Yer: -";
    }
}