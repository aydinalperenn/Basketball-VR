using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BallShotTracker : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private ShotResultUI shotResultUI;
    [SerializeField] private ShotLogger shotLogger;
    [SerializeField] private TwoHandBasketballGrab twoHandGrab;

    [Header("Ayarlar")]
    [SerializeField] private float releaseSampleDelay = 0.02f;
    [SerializeField] private float shotTimeout = 5f;
    [SerializeField] private float respawnDelay = 2f;

    private bool wasHeldLastFrame;
    private bool releasePending;
    private bool shotActive;
    private bool shotFinished;
    private bool scored;
    private bool waitingRespawn;

    private float releaseWaitTimer;
    private float shotTimer;
    private float respawnTimer;

    private float releaseSpeed;
    private float releaseAngle;

    private string firstHitObject = "Yok";
    private string finalResult = "Yok";

    private int shotId = 0;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        if (ballRigidbody == null)
            ballRigidbody = GetComponent<Rigidbody>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (twoHandGrab == null)
            twoHandGrab = GetComponent<TwoHandBasketballGrab>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        bool isHeld = grabInteractable != null && grabInteractable.isSelected;

        // Top yeni tutulduğunda eski atış bilgilerini temizle.
        if (!wasHeldLastFrame && isHeld)
        {
            PrepareForNewAttempt();
        }

        // Top bırakıldığında her zaman şut başlatma.
        // Sadece TwoHandBasketballGrab bunu geçerli bir şut bırakışı olarak işaretlediyse başlat.
        if (wasHeldLastFrame && !isHeld)
        {
            bool validShotRelease = true;

            if (twoHandGrab != null)
            {
                validShotRelease = twoHandGrab.ConsumeValidShotRelease();
            }

            if (validShotRelease)
            {
                BeginReleaseSampling();
            }
        }

        wasHeldLastFrame = isHeld;

        // Aktif atışın süresini takip et.
        if (shotActive && !shotFinished)
        {
            shotTimer += Time.deltaTime;

            if (shotTimer >= shotTimeout)
            {
                FinishShot(false);
            }
        }

        // Atış bittikten sonra topu tekrar spawn noktasına gönder.
        if (waitingRespawn)
        {
            respawnTimer += Time.deltaTime;

            if (respawnTimer >= respawnDelay)
            {
                RespawnBall();
            }
        }
    }

    private void FixedUpdate()
    {
        // Top bırakıldıktan sonra kısa bir fizik beklemesi yaparak
        // daha stabil hız verisi alıyoruz.
        if (releasePending)
        {
            releaseWaitTimer += Time.fixedDeltaTime;

            if (releaseWaitTimer >= releaseSampleDelay)
            {
                CaptureReleaseDataAndStartShot();
            }
        }
    }

    private void PrepareForNewAttempt()
    {
        waitingRespawn = false;
        respawnTimer = 0f;

        releasePending = false;
        releaseWaitTimer = 0f;

        shotActive = false;
        shotFinished = false;
        scored = false;
        shotTimer = 0f;

        releaseSpeed = 0f;
        releaseAngle = 0f;

        firstHitObject = "Yok";
        finalResult = "Yok";

        if (shotResultUI != null)
        {
            shotResultUI.ClearUI();
        }
    }

    private void BeginReleaseSampling()
    {
        releasePending = true;
        releaseWaitTimer = 0f;
    }

    private void CaptureReleaseDataAndStartShot()
    {
        releasePending = false;
        releaseWaitTimer = 0f;

        Vector3 velocity = ballRigidbody.linearVelocity;

        // Elden çıktığı andaki hız
        releaseSpeed = velocity.magnitude;

        // Elden çıktığı andaki açı
        releaseAngle = CalculateReleaseAngle(velocity);

        shotId++;
        shotActive = true;
        shotFinished = false;
        scored = false;
        shotTimer = 0f;
    }

    private float CalculateReleaseAngle(Vector3 velocity)
    {
        float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        return Mathf.Atan2(velocity.y, horizontalSpeed) * Mathf.Rad2Deg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!shotActive || shotFinished)
            return;

        string hitName = GetHitName(collision.collider);

        // Sadece istediğimiz yüzeylerden biriyse ilk temas bilgisini kaydet.
        if (firstHitObject == "Yok" && hitName != "Yok")
        {
            firstHitObject = hitName;
        }

        // Top zemine değdiyse ve henüz sayı olmadıysa atışı kaçtı olarak bitir.
        if (!scored && hitName == "Zemin")
        {
            FinishShot(false);
        }
    }

    private string GetHitName(Collider otherCollider)
    {
        if (HasTagInParents(otherCollider.transform, "Rim"))
            return "Pota Çemberi";

        if (HasTagInParents(otherCollider.transform, "Backboard"))
            return "Panya";

        if (HasTagInParents(otherCollider.transform, "Floor"))
            return "Zemin";

        return "Yok";
    }

    private bool HasTagInParents(Transform current, string targetTag)
    {
        while (current != null)
        {
            if (current.CompareTag(targetTag))
                return true;

            current = current.parent;
        }

        return false;
    }

    public void RegisterScore()
    {
        if (!shotActive || shotFinished)
            return;

        scored = true;
        FinishShot(true);
    }

    private void FinishShot(bool isScore)
    {
        if (shotFinished)
            return;

        shotFinished = true;
        shotActive = false;
        scored = isScore;

        finalResult = isScore ? "Sayı" : "Kaçtı";

        if (shotResultUI != null)
        {
            shotResultUI.ShowShotResult(releaseSpeed, releaseAngle, isScore, firstHitObject);
        }

        if (shotLogger != null)
        {
            shotLogger.LogShot(shotId, releaseSpeed, releaseAngle, isScore, firstHitObject, finalResult);
        }

        waitingRespawn = true;
        respawnTimer = 0f;
    }

    private void RespawnBall()
    {
        waitingRespawn = false;
        respawnTimer = 0f;

        releasePending = false;
        releaseWaitTimer = 0f;

        shotActive = false;
        shotFinished = false;
        scored = false;
        shotTimer = 0f;

        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        if (ballSpawnPoint != null)
        {
            transform.SetPositionAndRotation(ballSpawnPoint.position, ballSpawnPoint.rotation);
        }
        else
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
        }

        ballRigidbody.Sleep();
    }
}