using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TwoHandBasketballGrab : MonoBehaviour
{
    private enum HandSide
    {
        None,
        Left,
        Right
    }

    [Header("Referanslar")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private Rigidbody ballRigidbody;

    [Tooltip("Hierarchy'deki Left Controller objesini ver.")]
    [SerializeField] private Transform leftControllerRoot;

    [Tooltip("Hierarchy'deki Right Controller objesini ver.")]
    [SerializeField] private Transform rightControllerRoot;

    [Header("Top Üzerindeki El Noktaları")]
    [SerializeField] private Transform rightPrimaryAnchor;
    [SerializeField] private Transform leftSupportAnchor;
    [SerializeField] private Transform leftPrimaryAnchor;
    [SerializeField] private Transform rightSupportAnchor;

    private HandSide primaryHand = HandSide.None;

    private IXRSelectInteractor leftInteractor;
    private IXRSelectInteractor rightInteractor;

    private bool leftWasSelecting;
    private bool rightWasSelecting;

    // Bu bayrak, BallShotTracker'a "bu bırakış gerçek şut bırakışı mı?" bilgisini verir.
    private bool pendingValidShotRelease;

    // Bu bayrak, top tamamen bırakıldıktan sonra release velocity uygulamak için kullanılır.
    private bool pendingVelocityApply;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private bool hasPreviousPose;

    private Vector3 sampledLinearVelocity;
    private Vector3 sampledAngularVelocity;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (ballRigidbody == null)
            ballRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateCurrentInteractors();

        bool leftIsSelecting = leftInteractor != null;
        bool rightIsSelecting = rightInteractor != null;

        AssignPrimaryHand(leftIsSelecting, rightIsSelecting);
        HandlePrimaryHandRelease(leftIsSelecting, rightIsSelecting);

        // Force release yaptıysak selection listesi değişmiş olabilir.
        UpdateCurrentInteractors();

        leftIsSelecting = leftInteractor != null;
        rightIsSelecting = rightInteractor != null;

        // Gerçek şut bırakışıysa, top tamamen serbest kaldığı anda kendi örneklediğimiz hızı uygula.
        if (pendingVelocityApply && !leftIsSelecting && !rightIsSelecting)
        {
            ballRigidbody.linearVelocity = sampledLinearVelocity;
            ballRigidbody.angularVelocity = sampledAngularVelocity;
            pendingVelocityApply = false;
        }

        // Top tamamen bırakıldıysa el durumlarını sıfırla.
        if (!leftIsSelecting && !rightIsSelecting)
        {
            primaryHand = HandSide.None;
            hasPreviousPose = false;
        }

        leftWasSelecting = leftIsSelecting;
        rightWasSelecting = rightIsSelecting;
    }

    private void LateUpdate()
    {
        if (grabInteractable == null || !grabInteractable.isSelected)
            return;

        UpdateCurrentInteractors();

        // Ana el sağ el ise
        if (primaryHand == HandSide.Right && rightInteractor != null)
        {
            if (leftInteractor != null)
            {
                // Sağ ana el + sol destek el
                ApplyTwoHandPose(
                    rightControllerRoot, rightPrimaryAnchor,
                    leftControllerRoot, leftSupportAnchor
                );
            }
            else
            {
                // Sadece sağ ana el topu tutuyor
                ApplySingleHandPose(rightControllerRoot, rightPrimaryAnchor);
            }
        }
        // Ana el sol el ise
        else if (primaryHand == HandSide.Left && leftInteractor != null)
        {
            if (rightInteractor != null)
            {
                // Sol ana el + sağ destek el
                ApplyTwoHandPose(
                    leftControllerRoot, leftPrimaryAnchor,
                    rightControllerRoot, rightSupportAnchor
                );
            }
            else
            {
                // Sadece sol ana el topu tutuyor
                ApplySingleHandPose(leftControllerRoot, leftPrimaryAnchor);
            }
        }
    }

    // BallShotTracker buradan "bu bırakış geçerli şut bırakışı mı?" bilgisini alır.
    public bool ConsumeValidShotRelease()
    {
        bool value = pendingValidShotRelease;
        pendingValidShotRelease = false;
        return value;
    }

    private void AssignPrimaryHand(bool leftIsSelecting, bool rightIsSelecting)
    {
        if (primaryHand != HandSide.None)
            return;

        // Topu ilk hangi el tuttuysa o ana el olsun.
        if (!leftWasSelecting && leftIsSelecting)
        {
            primaryHand = HandSide.Left;
            pendingValidShotRelease = false;
            pendingVelocityApply = false;
            return;
        }

        if (!rightWasSelecting && rightIsSelecting)
        {
            primaryHand = HandSide.Right;
            pendingValidShotRelease = false;
            pendingVelocityApply = false;
        }
    }

    private void HandlePrimaryHandRelease(bool leftIsSelecting, bool rightIsSelecting)
    {
        bool primaryReleased = false;
        bool validTwoHandShot = false;

        // Ana el sağ eldi ve sağ el bırakıldı.
        if (primaryHand == HandSide.Right && rightWasSelecting && !rightIsSelecting)
        {
            primaryReleased = true;
            validTwoHandShot = leftWasSelecting && rightWasSelecting;

            // Geçerli şutsa destek eli de otomatik bıraktır.
            if (validTwoHandShot && leftInteractor != null)
            {
                ForceRelease(leftInteractor);
            }
        }
        // Ana el sol eldi ve sol el bırakıldı.
        else if (primaryHand == HandSide.Left && leftWasSelecting && !leftIsSelecting)
        {
            primaryReleased = true;
            validTwoHandShot = leftWasSelecting && rightWasSelecting;

            // Geçerli şutsa destek eli de otomatik bıraktır.
            if (validTwoHandShot && rightInteractor != null)
            {
                ForceRelease(rightInteractor);
            }
        }

        if (primaryReleased)
        {
            pendingValidShotRelease = validTwoHandShot;
            pendingVelocityApply = validTwoHandShot;

            if (!validTwoHandShot)
            {
                sampledLinearVelocity = Vector3.zero;
                sampledAngularVelocity = Vector3.zero;
            }
        }
    }

    private void ForceRelease(IXRSelectInteractor interactor)
    {
        if (interactionManager == null || interactor == null || grabInteractable == null)
            return;

        interactionManager.SelectExit(interactor, grabInteractable);
    }

    private void UpdateCurrentInteractors()
    {
        leftInteractor = null;
        rightInteractor = null;

        if (grabInteractable == null)
            return;

        var selecting = grabInteractable.interactorsSelecting;

        for (int i = 0; i < selecting.Count; i++)
        {
            IXRSelectInteractor interactor = selecting[i];

            if (BelongsToController(interactor, leftControllerRoot))
            {
                leftInteractor = interactor;
            }
            else if (BelongsToController(interactor, rightControllerRoot))
            {
                rightInteractor = interactor;
            }
        }
    }

    private bool BelongsToController(IXRSelectInteractor interactor, Transform controllerRoot)
    {
        if (interactor == null || controllerRoot == null)
            return false;

        Component interactorComponent = interactor as Component;
        if (interactorComponent == null)
            return false;

        Transform t = interactorComponent.transform;

        return t == controllerRoot || t.IsChildOf(controllerRoot);
    }

    private void ApplySingleHandPose(Transform handTransform, Transform anchor)
    {
        if (handTransform == null || anchor == null)
            return;

        Quaternion targetRotation = handTransform.rotation * Quaternion.Inverse(anchor.localRotation);
        Vector3 targetPosition = handTransform.position - (targetRotation * anchor.localPosition);

        ApplyPose(targetPosition, targetRotation);
    }

    private void ApplyTwoHandPose(
        Transform primaryHandTransform, Transform primaryAnchor,
        Transform supportHandTransform, Transform supportAnchor)
    {
        if (primaryHandTransform == null || primaryAnchor == null ||
            supportHandTransform == null || supportAnchor == null)
            return;

        Vector3 localAxis = supportAnchor.localPosition - primaryAnchor.localPosition;
        Vector3 worldAxis = supportHandTransform.position - primaryHandTransform.position;

        // Eller çok yakınsa tek el davranışına düş.
        if (localAxis.sqrMagnitude < 0.000001f || worldAxis.sqrMagnitude < 0.000001f)
        {
            ApplySingleHandPose(primaryHandTransform, primaryAnchor);
            return;
        }

        Vector3 localAxisNormalized = localAxis.normalized;
        Vector3 worldAxisNormalized = worldAxis.normalized;

        // Önce top üzerindeki iki el noktası arasındaki doğrultuyu,
        // gerçek iki el arasındaki doğrultuya hizala.
        Quaternion alignAxisRotation = Quaternion.FromToRotation(localAxisNormalized, worldAxisNormalized);

        // Sonra primary hand'in yukarı yönünü mümkün olduğunca koruyacak bir twist ekle.
        Vector3 currentUp = alignAxisRotation * (primaryAnchor.localRotation * Vector3.up);
        Vector3 projectedCurrentUp = Vector3.ProjectOnPlane(currentUp, worldAxisNormalized);
        Vector3 projectedDesiredUp = Vector3.ProjectOnPlane(primaryHandTransform.up, worldAxisNormalized);

        Quaternion twistRotation = Quaternion.identity;

        if (projectedCurrentUp.sqrMagnitude > 0.000001f && projectedDesiredUp.sqrMagnitude > 0.000001f)
        {
            float twistAngle = Vector3.SignedAngle(
                projectedCurrentUp.normalized,
                projectedDesiredUp.normalized,
                worldAxisNormalized
            );

            twistRotation = Quaternion.AngleAxis(twistAngle, worldAxisNormalized);
        }

        Quaternion targetRotation = twistRotation * alignAxisRotation;
        Vector3 targetPosition = primaryHandTransform.position - (targetRotation * primaryAnchor.localPosition);

        ApplyPose(targetPosition, targetRotation);
    }

    private void ApplyPose(Vector3 targetPosition, Quaternion targetRotation)
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        if (hasPreviousPose)
        {
            sampledLinearVelocity = (targetPosition - previousPosition) / dt;

            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(previousRotation);
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);

            if (angleInDegrees > 180f)
                angleInDegrees -= 360f;

            sampledAngularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad / dt);
        }
        else
        {
            sampledLinearVelocity = Vector3.zero;
            sampledAngularVelocity = Vector3.zero;
            hasPreviousPose = true;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);

        previousPosition = targetPosition;
        previousRotation = targetRotation;
    }
}