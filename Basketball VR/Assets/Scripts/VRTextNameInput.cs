using System.Collections;
using TMPro;
using UnityEngine;

public class VRTextNameInput : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text statusText;

    [Header("Settings")]
    [SerializeField] private int maxCharacterCount = 20;
    [SerializeField] private float statusVisibleTime = 2f;

    private string currentName = "";
    private Coroutine statusCoroutine;

    private void OnEnable()
    {
        LoadSavedName();
        HideStatus();
    }

    private void Start()
    {
        LoadSavedName();
        HideStatus();
    }

    private void LoadSavedName()
    {
        if (PlayerSessionManager.Instance != null)
        {
            currentName = PlayerSessionManager.Instance.GetDisplayNameForMenu();
        }
        else
        {
            currentName = "";
        }

        RefreshUI();
    }

    public void AddCharacter(string character)
    {
        if (string.IsNullOrEmpty(character))
            return;

        if (currentName.Length >= maxCharacterCount)
            return;

        currentName += character.ToUpper();
        RefreshUI();
    }

    public void Backspace()
    {
        if (currentName.Length <= 0)
            return;

        currentName = currentName.Substring(0, currentName.Length - 1);
        RefreshUI();
    }

    public void ClearName()
    {
        currentName = "";
        RefreshUI();
    }

    public void SaveName()
    {
        if (string.IsNullOrWhiteSpace(currentName))
        {
            ShowStatus("Enter name");
            return;
        }

        if (PlayerSessionManager.Instance != null)
        {
            PlayerSessionManager.Instance.SetPlayerName(currentName);

            // Saved name may be cleaned, so reload it after saving.
            currentName = PlayerSessionManager.Instance.GetDisplayNameForMenu();
            RefreshUI();

            ShowStatus("Saved");
        }
        else
        {
            ShowStatus("Session manager missing");
        }
    }

    private void RefreshUI()
    {
        if (playerNameText == null)
            return;

        if (string.IsNullOrEmpty(currentName))
            playerNameText.text = "Player: -";
        else
            playerNameText.text = "Player: " + currentName.ToUpper();
    }

    private void ShowStatus(string message)
    {
        if (statusText == null)
            return;

        statusText.text = message;
        statusText.gameObject.SetActive(true);

        if (statusCoroutine != null)
            StopCoroutine(statusCoroutine);

        statusCoroutine = StartCoroutine(HideStatusAfterDelay());
    }

    private IEnumerator HideStatusAfterDelay()
    {
        yield return new WaitForSeconds(statusVisibleTime);
        HideStatus();
    }

    private void HideStatus()
    {
        if (statusText != null)
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
    }
}