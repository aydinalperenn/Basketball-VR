using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSpeedButtonUI : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [SerializeField] private TMP_Text speedValueText;

    [Header("Button Referanslarý")]
    [SerializeField] private Button decreaseLargeButton;
    [SerializeField] private Button decreaseSmallButton;
    [SerializeField] private Button increaseSmallButton;
    [SerializeField] private Button increaseLargeButton;

    [Header("Ayarlar")]
    [SerializeField] private float smallStep = 0.01f;
    [SerializeField] private float largeStep = 0.10f;
    [SerializeField] private string labelPrefix = "Top Hýzý: x";

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void IncreaseSmall()
    {
        ChangeSpeed(smallStep);
    }

    public void IncreaseLarge()
    {
        ChangeSpeed(largeStep);
    }

    public void DecreaseSmall()
    {
        ChangeSpeed(-smallStep);
    }

    public void DecreaseLarge()
    {
        ChangeSpeed(-largeStep);
    }

    public void ResetSpeed()
    {
        if (BallSpeedSettings.Instance != null)
        {
            BallSpeedSettings.Instance.ResetToDefault();
        }

        RefreshUI();
    }

    private void ChangeSpeed(float amount)
    {
        if (BallSpeedSettings.Instance == null)
        {
            Debug.LogWarning("BallSpeedSettings sahnede bulunamadý.");
            return;
        }

        BallSpeedSettings.Instance.AddMultiplier(amount);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (BallSpeedSettings.Instance == null)
            return;

        float value = BallSpeedSettings.Instance.CurrentMultiplier;

        if (speedValueText != null)
        {
            speedValueText.text = labelPrefix + value.ToString("F2");
        }

        UpdateButtonStates(value);
    }

    private void UpdateButtonStates(float value)
    {
        if (BallSpeedSettings.Instance == null)
            return;

        float min = BallSpeedSettings.Instance.MinMultiplier;
        float max = BallSpeedSettings.Instance.MaxMultiplier;

        bool canDecrease = value > min;
        bool canIncrease = value < max;

        if (decreaseLargeButton != null)
            decreaseLargeButton.interactable = canDecrease;

        if (decreaseSmallButton != null)
            decreaseSmallButton.interactable = canDecrease;

        if (increaseSmallButton != null)
            increaseSmallButton.interactable = canIncrease;

        if (increaseLargeButton != null)
            increaseLargeButton.interactable = canIncrease;
    }
}