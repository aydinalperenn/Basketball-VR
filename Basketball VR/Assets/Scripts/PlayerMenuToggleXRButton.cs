using UnityEngine;
using UnityEngine.XR;

public class PlayerMenuToggleXRButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuCanvas;

    [Header("UI Ray Objeleri")]
    [SerializeField] private GameObject rightUiRayObject;
    [SerializeField] private GameObject leftUiRayObject;

    [Header("Menü Açýkken Kapatýlacak Diðer Ray / Interactor Objeleri")]
    [Tooltip("A'ya basýnca ikinci ray çýkaran eski ray, teleport ray veya Near-Far line visual objelerini buraya ekle.")]
    [SerializeField] private GameObject[] objectsToDisableWhileMenuOpen;

    [Header("Oyuncu / Kamera")]
    [SerializeField] private Transform playerCamera;

    [Header("Menü Konumu")]
    [SerializeField] private float menuDistance = 1.5f;
    [SerializeField] private float verticalOffset = -0.15f;

    [Header("Buton Ayarý")]
    [SerializeField] private XRNode controllerNode = XRNode.RightHand;

    [Tooltip("Quest sað controller B tuþu için açýk kalsýn.")]
    [SerializeField] private bool useSecondaryButton = true;

    private bool isOpen;
    private bool wasButtonPressedLastFrame;

    private void Start()
    {
        CloseMenu();
    }

    private void Update()
    {
        bool isButtonPressed = ReadButton();

        if (isButtonPressed && !wasButtonPressedLastFrame)
        {
            if (isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        wasButtonPressedLastFrame = isButtonPressed;
    }

    private bool ReadButton()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);

        if (!device.isValid)
            return false;

        bool buttonValue = false;

        if (useSecondaryButton)
            device.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonValue);
        else
            device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonValue);

        return buttonValue;
    }

    private void OpenMenu()
    {
        if (menuCanvas == null || playerCamera == null)
            return;

        isOpen = true;

        menuCanvas.SetActive(true);

        if (rightUiRayObject != null)
            rightUiRayObject.SetActive(true);

        if (leftUiRayObject != null)
            leftUiRayObject.SetActive(true);

        SetOtherRayObjectsActive(false);

        PositionMenuInFrontOfPlayer();
    }

    private void CloseMenu()
    {
        isOpen = false;

        if (menuCanvas != null)
            menuCanvas.SetActive(false);

        if (rightUiRayObject != null)
            rightUiRayObject.SetActive(false);

        if (leftUiRayObject != null)
            leftUiRayObject.SetActive(false);

        SetOtherRayObjectsActive(true);
    }

    private void SetOtherRayObjectsActive(bool isActive)
    {
        if (objectsToDisableWhileMenuOpen == null)
            return;

        for (int i = 0; i < objectsToDisableWhileMenuOpen.Length; i++)
        {
            if (objectsToDisableWhileMenuOpen[i] != null)
                objectsToDisableWhileMenuOpen[i].SetActive(isActive);
        }
    }

    private void PositionMenuInFrontOfPlayer()
    {
        Vector3 forward = playerCamera.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = playerCamera.forward;

        forward.Normalize();

        Vector3 targetPosition = playerCamera.position + forward * menuDistance;
        targetPosition.y = playerCamera.position.y + verticalOffset;

        menuCanvas.transform.position = targetPosition;
        menuCanvas.transform.rotation = Quaternion.LookRotation(forward);
    }
}