using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BallShotTracker : MonoBehaviour
{
    private struct HitCandidate
    {
        public bool valid;
        public int frameNumber;
        public float distanceToBallCenterSqr;
        public string hitObject;
        public string hitRegionDisplay;
        public string hitRegionCsv;
        public string colliderName;
        public Vector3 worldPoint;
    }

    [Header("Referanslar")]
    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private ShotResultUI shotResultUI;
    [SerializeField] private ShotLogger shotLogger;
    [SerializeField] private HitMarkerFeedback hitMarkerFeedback;

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
    private int shotId = 0;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool firstHitCommitted;
    private string firstHitObject = "Yok";
    private string firstHitRegionDisplay = "Yok";
    private string firstHitRegionCsv = "Yok";
    private string firstHitColliderName = "Yok";
    private Vector3 firstHitWorldPosition;
    private bool hasFirstHitWorldPosition;

    private bool hasPendingHit;
    private HitCandidate pendingHit;

    private void Awake()
    {
        if (ballRigidbody == null)
            ballRigidbody = GetComponent<Rigidbody>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        bool isHeld = grabInteractable != null && grabInteractable.isSelected;

        if (!wasHeldLastFrame && isHeld)
        {
            PrepareForNewAttempt();
        }

        if (wasHeldLastFrame && !isHeld)
        {
            BeginReleaseSampling();
        }

        wasHeldLastFrame = isHeld;

        CommitPendingHitIfReady();

        if (shotActive && !shotFinished)
        {
            shotTimer += Time.deltaTime;

            if (shotTimer >= shotTimeout)
            {
                FinishShot(false);
            }
        }

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

        firstHitCommitted = false;
        firstHitObject = "Yok";
        firstHitRegionDisplay = "Yok";
        firstHitRegionCsv = "Yok";
        firstHitColliderName = "Yok";
        firstHitWorldPosition = Vector3.zero;
        hasFirstHitWorldPosition = false;

        hasPendingHit = false;
        pendingHit = default;

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

        // Eğer Unity sürümünde velocity kullanılıyorsa bunu velocity yap
        Vector3 velocity = ballRigidbody.linearVelocity;

        releaseSpeed = velocity.magnitude;
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

        BallHitRegion hitRegion = collision.collider.GetComponentInParent<BallHitRegion>();

        string hitObject = "";
        string regionDisplay = "";
        string regionCsv = "";
        string colliderName = collision.collider.name;

        if (hitRegion != null)
        {
            hitObject = hitRegion.HitObject;
            regionDisplay = hitRegion.DisplayName;
            regionCsv = hitRegion.CsvRegionName;
        }
        else
        {
            // Region scripti yoksa ama zeminse fallback
            if (HasTagInParents(collision.collider.transform, "Floor"))
            {
                hitObject = "Zemin";
                regionDisplay = "Zemin";
                regionCsv = "Floor";
            }
            else
            {
                return;
            }
        }

        Vector3 bestHitPoint = GetBestContactPointClosestToBallCenter(collision);
        float distanceSqr = (bestHitPoint - transform.position).sqrMagnitude;

        HitCandidate candidate = new HitCandidate
        {
            valid = true,
            frameNumber = Time.frameCount,
            distanceToBallCenterSqr = distanceSqr,
            hitObject = hitObject,
            hitRegionDisplay = regionDisplay,
            hitRegionCsv = regionCsv,
            colliderName = colliderName,
            worldPoint = bestHitPoint
        };

        if (!firstHitCommitted)
        {
            if (!hasPendingHit)
            {
                hasPendingHit = true;
                pendingHit = candidate;
            }
            else
            {
                // Aynı frame içinde birden fazla collider geldiyse top merkezine en yakın olanı al
                if (candidate.frameNumber == pendingHit.frameNumber)
                {
                    if (candidate.distanceToBallCenterSqr < pendingHit.distanceToBallCenterSqr)
                    {
                        pendingHit = candidate;
                    }
                }
            }
        }

        // Eğer zemin teması geldiyse atışı kaçtı olarak bitir
        if (!scored && hitObject == "Zemin")
        {
            CommitPendingHitNow();
            FinishShot(false);
        }
    }

    private Vector3 GetBestContactPointClosestToBallCenter(Collision collision)
    {
        ContactPoint[] contacts = collision.contacts;

        if (contacts == null || contacts.Length == 0)
            return collision.collider.bounds.ClosestPoint(transform.position);

        Vector3 bestPoint = contacts[0].point;
        float bestDistance = (bestPoint - transform.position).sqrMagnitude;

        for (int i = 1; i < contacts.Length; i++)
        {
            float distance = (contacts[i].point - transform.position).sqrMagnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPoint = contacts[i].point;
            }
        }

        return bestPoint;
    }

    private void CommitPendingHitIfReady()
    {
        if (firstHitCommitted || !hasPendingHit)
            return;

        // Bir frame geçince pending hit'i kesinleştir
        if (Time.frameCount > pendingHit.frameNumber)
        {
            CommitPendingHitNow();
        }
    }

    private void CommitPendingHitNow()
    {
        if (firstHitCommitted || !hasPendingHit)
            return;

        firstHitCommitted = true;
        firstHitObject = pendingHit.hitObject;
        firstHitRegionDisplay = pendingHit.hitRegionDisplay;
        firstHitRegionCsv = pendingHit.hitRegionCsv;
        firstHitColliderName = pendingHit.colliderName;
        firstHitWorldPosition = pendingHit.worldPoint;
        hasFirstHitWorldPosition = true;

        if (hitMarkerFeedback != null)
        {
            hitMarkerFeedback.ShowMarker(firstHitWorldPosition, firstHitRegionDisplay);
        }

        hasPendingHit = false;
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
        CommitPendingHitNow();
        FinishShot(true);
    }

    private void FinishShot(bool isScore)
    {
        if (shotFinished)
            return;

        CommitPendingHitNow();

        shotFinished = true;
        shotActive = false;
        scored = isScore;

        string finalResult = isScore ? "Sayı" : "Kaçtı";

        string displayHitObject = firstHitObject == "Yok" ? "Temassız" : firstHitObject;
        string displayHitRegion = firstHitRegionDisplay == "Yok" ? displayHitObject : firstHitRegionDisplay;

        if (shotResultUI != null)
        {
            shotResultUI.ShowShotResult(
                releaseSpeed,
                releaseAngle,
                isScore,
                displayHitObject,
                displayHitRegion
            );
        }

        if (shotLogger != null)
        {
            shotLogger.LogShot(
                shotId,
                releaseSpeed,
                releaseAngle,
                isScore,
                displayHitObject,
                firstHitRegionCsv == "Yok" ? displayHitObject : firstHitRegionCsv,
                firstHitColliderName,
                firstHitWorldPosition,
                hasFirstHitWorldPosition,
                finalResult
            );
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