using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SingleHandVisualAndLock : MonoBehaviour
{
    private enum HandSide
    {
        None,
        Left,
        Right
    }

    [Header("Temel Referanslar")]
    [SerializeField] private XRGrabInteractable grabInteractable;

    [Header("Controller Root Referanslarý")]
    [SerializeField] private Transform leftControllerRoot;
    [SerializeField] private Transform rightControllerRoot;

    [Header("Gerçek El Görselleri")]
    [SerializeField] private GameObject leftHandVisual;
    [SerializeField] private GameObject rightHandVisual;

    [Header("Gerçek Grab Yapan Interactor Bileþenleri")]
    [Tooltip("Left Controller üzerindeki Near-Far Interactor veya gerçek grab interactor.")]
    [SerializeField] private Behaviour leftSelectInteractor;

    [Tooltip("Right Controller üzerindeki Near-Far Interactor veya gerçek grab interactor.")]
    [SerializeField] private Behaviour rightSelectInteractor;

    [Header("Ayarlar")]
    [SerializeField] private float reenableDelayAfterRelease = 0.10f;

    private HandSide holdingHand = HandSide.None;
    private HandSide delayedReenableHand = HandSide.None;

    private bool wasHeldLastFrame;
    private float reenableTimer;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        ShowBothHands();
        EnableBothInteractors();
    }

    private void OnEnable()
    {
        ShowBothHands();
        EnableBothInteractors();
    }

    private void OnDisable()
    {
        ShowBothHands();
        EnableBothInteractors();
    }

    private void Update()
    {
        UpdateDelayedReenable();

        bool isHeld = grabInteractable != null && grabInteractable.isSelected;

        // Top ilk tutulduðu anda hangi elin tuttuðunu bul
        if (!wasHeldLastFrame && isHeld)
        {
            DetermineHoldingHand();
        }

        if (isHeld)
        {
            if (holdingHand == HandSide.None)
            {
                DetermineHoldingHand();
            }

            ApplyHeldState();
        }
        else
        {
            if (wasHeldLastFrame)
            {
                StartDelayedReenableForOppositeHand();
            }

            holdingHand = HandSide.None;
            ShowBothHands();
        }

        wasHeldLastFrame = isHeld;
    }

    private void DetermineHoldingHand()
    {
        holdingHand = HandSide.None;

        if (grabInteractable == null || grabInteractable.interactorsSelecting.Count == 0)
            return;

        IXRSelectInteractor selectingInteractor = grabInteractable.interactorsSelecting[0];
        if (selectingInteractor == null)
            return;

        if (BelongsToController(selectingInteractor, leftControllerRoot))
        {
            holdingHand = HandSide.Left;
        }
        else if (BelongsToController(selectingInteractor, rightControllerRoot))
        {
            holdingHand = HandSide.Right;
        }
    }

    private void ApplyHeldState()
    {
        // Sol el topu tutuyorsa sað el görünmesin ve sað interactor kapan­sýn
        if (holdingHand == HandSide.Left)
        {
            SetActiveSafe(leftHandVisual, true);
            SetActiveSafe(rightHandVisual, false);

            SetInteractorEnabled(leftSelectInteractor, true);
            SetInteractorEnabled(rightSelectInteractor, false);
        }
        // Sað el topu tutuyorsa sol el görünmesin ve sol interactor kapan­sýn
        else if (holdingHand == HandSide.Right)
        {
            SetActiveSafe(leftHandVisual, false);
            SetActiveSafe(rightHandVisual, true);

            SetInteractorEnabled(leftSelectInteractor, false);
            SetInteractorEnabled(rightSelectInteractor, true);
        }
        else
        {
            ShowBothHands();
            EnableBothInteractors();
        }
    }

    private void StartDelayedReenableForOppositeHand()
    {
        if (holdingHand == HandSide.Left)
        {
            delayedReenableHand = HandSide.Right;
            reenableTimer = reenableDelayAfterRelease;

            SetInteractorEnabled(leftSelectInteractor, true);
            SetInteractorEnabled(rightSelectInteractor, false);
        }
        else if (holdingHand == HandSide.Right)
        {
            delayedReenableHand = HandSide.Left;
            reenableTimer = reenableDelayAfterRelease;

            SetInteractorEnabled(leftSelectInteractor, false);
            SetInteractorEnabled(rightSelectInteractor, true);
        }
        else
        {
            delayedReenableHand = HandSide.None;
            reenableTimer = 0f;
            EnableBothInteractors();
        }
    }

    private void UpdateDelayedReenable()
    {
        if (delayedReenableHand == HandSide.None)
            return;

        reenableTimer -= Time.deltaTime;

        if (reenableTimer <= 0f)
        {
            delayedReenableHand = HandSide.None;
            reenableTimer = 0f;
            EnableBothInteractors();
        }
    }

    private void ShowBothHands()
    {
        SetActiveSafe(leftHandVisual, true);
        SetActiveSafe(rightHandVisual, true);
    }

    private void EnableBothInteractors()
    {
        SetInteractorEnabled(leftSelectInteractor, true);
        SetInteractorEnabled(rightSelectInteractor, true);
    }

    private void SetActiveSafe(GameObject target, bool isActive)
    {
        if (target == null)
            return;

        if (target.activeSelf != isActive)
        {
            target.SetActive(isActive);
        }
    }

    private void SetInteractorEnabled(Behaviour interactorBehaviour, bool isEnabled)
    {
        if (interactorBehaviour == null)
            return;

        if (interactorBehaviour.enabled != isEnabled)
        {
            interactorBehaviour.enabled = isEnabled;
        }
    }

    private bool BelongsToController(IXRSelectInteractor interactor, Transform controllerRoot)
    {
        if (interactor == null || controllerRoot == null)
            return false;

        Component component = interactor as Component;
        if (component == null)
            return false;

        Transform t = component.transform;
        return t == controllerRoot || t.IsChildOf(controllerRoot);
    }
}