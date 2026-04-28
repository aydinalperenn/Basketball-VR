using UnityEngine;

public class BallSpeedSettings : MonoBehaviour
{
    public static BallSpeedSettings Instance { get; private set; }

    private const string PlayerPrefsKey = "BallSpeedMultiplier";

    [Header("Top Hýz Ayarý")]
    [SerializeField] private float defaultMultiplier = 1.45f;
    [SerializeField] private float minMultiplier = 0.80f;
    [SerializeField] private float maxMultiplier = 2.20f;

    private float currentMultiplier;

    public float CurrentMultiplier
    {
        get { return currentMultiplier; }
    }

    public float MinMultiplier
    {
        get { return minMultiplier; }
    }

    public float MaxMultiplier
    {
        get { return maxMultiplier; }
    }

    private void Awake()
    {
        Instance = this;

        currentMultiplier = PlayerPrefs.GetFloat(PlayerPrefsKey, defaultMultiplier);
        currentMultiplier = ClampAndRound(currentMultiplier);
    }

    public void SetMultiplier(float newMultiplier)
    {
        currentMultiplier = ClampAndRound(newMultiplier);

        PlayerPrefs.SetFloat(PlayerPrefsKey, currentMultiplier);
        PlayerPrefs.Save();

        Debug.Log("Ball speed multiplier: x" + currentMultiplier.ToString("F2"));
    }

    public void AddMultiplier(float amount)
    {
        SetMultiplier(currentMultiplier + amount);
    }

    public void ResetToDefault()
    {
        SetMultiplier(defaultMultiplier);
    }

    private float ClampAndRound(float value)
    {
        value = Mathf.Clamp(value, minMultiplier, maxMultiplier);

        // 1.45000005 gibi float hatalarýný engeller.
        value = Mathf.Round(value * 100f) / 100f;

        return value;
    }
}