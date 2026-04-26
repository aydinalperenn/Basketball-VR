using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRUIButtonClickWithA : MonoBehaviour
{
    [Header("XR Ray")]
    [SerializeField] private XRRayInteractor rayInteractor;

    [Header("Controller")]
    [SerializeField] private XRNode controllerNode = XRNode.RightHand;

    [Header("Buton")]
    [Tooltip("A tuþu için Primary Button açýk kalsýn.")]
    [SerializeField] private bool usePrimaryButton = true;

    private bool wasPressedLastFrame;

    private void Awake()
    {
        if (rayInteractor == null)
            rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void Update()
    {
        bool isPressed = ReadButton();

        // Tuþa yeni basýldýðý aný yakala
        if (isPressed && !wasPressedLastFrame)
        {
            TryClickCurrentUIButton();
        }

        wasPressedLastFrame = isPressed;
    }

    private bool ReadButton()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);

        if (!device.isValid)
            return false;

        bool value = false;

        if (usePrimaryButton)
            device.TryGetFeatureValue(CommonUsages.primaryButton, out value);
        else
            device.TryGetFeatureValue(CommonUsages.secondaryButton, out value);

        return value;
    }

    private void TryClickCurrentUIButton()
    {
        if (rayInteractor == null)
            return;

        // XR Ray Interactor þu anda UI üstünde mi?
        if (!rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
            return;

        GameObject hitObject = raycastResult.gameObject;

        if (hitObject == null)
            return;

        // Týklanan objenin kendisinde veya parent'ýnda Button olabilir.
        ExecuteEvents.ExecuteHierarchy(
            hitObject,
            new PointerEventData(EventSystem.current),
            ExecuteEvents.pointerClickHandler
        );
    }
}