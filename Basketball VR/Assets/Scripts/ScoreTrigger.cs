using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    [SerializeField] private float minDownwardVelocity = -0.1f;

    private void OnTriggerEnter(Collider other)
    {
        BallShotTracker tracker = other.GetComponent<BallShotTracker>();

        if (tracker == null)
            tracker = other.GetComponentInParent<BallShotTracker>();

        if (tracker == null)
            return;

        Rigidbody ballRigidbody = other.attachedRigidbody;

        if (ballRigidbody == null)
            ballRigidbody = tracker.GetComponent<Rigidbody>();

        if (ballRigidbody == null)
            return;

        // Sadece top aşağı doğru potadan geçerken sayı kabul edilir.
        if (ballRigidbody.linearVelocity.y < minDownwardVelocity)
        {
            tracker.RegisterScore();
        }
    }
}