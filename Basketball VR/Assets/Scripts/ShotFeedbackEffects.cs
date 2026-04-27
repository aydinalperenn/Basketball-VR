using System.Collections;
using TMPro;
using UnityEngine;

public class ShotFeedbackEffects : MonoBehaviour
{
    [Header("Ortak Yazý")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Transform feedbackTextPoint;

    [Header("Basket Efekti")]
    [SerializeField] private ParticleSystem scoreParticle;
    [SerializeField] private string scoreText = "SAYI!";

    [Header("Kaçýrma Efekti")]
    [SerializeField] private ParticleSystem missParticlePrefab;
    [SerializeField] private string missText = "KAÇTI!";

    [Header("Animasyon Ayarlarý")]
    [SerializeField] private float textLifeTime = 1.5f;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float peakScale = 1.2f;
    [SerializeField] private float endScale = 0.05f;

    [Header("Kaçýrma Particle Ayarý")]
    [SerializeField] private Vector3 missParticleOffset = new Vector3(0f, 0.05f, 0f);

    private Coroutine activeTextRoutine;

    private void Awake()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    public void PlayScoreEffect()
    {
        if (scoreParticle != null)
        {
            scoreParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            scoreParticle.Play();
        }

        ShowAnimatedText(scoreText);
    }

    public void PlayMissEffect(string hitRegion, Vector3 hitPoint, bool hasHitPoint)
    {
        if (missParticlePrefab != null && hasHitPoint)
        {
            ParticleSystem particle = Instantiate(
                missParticlePrefab,
                hitPoint + missParticleOffset,
                Quaternion.identity
            );

            particle.Play();

            Destroy(particle.gameObject, 3f);
        }

        string label = missText;

        if (!string.IsNullOrWhiteSpace(hitRegion) && hitRegion != "Yok" && hitRegion != "Temassýz")
        {
            label = missText + "\n" + hitRegion;
        }

        ShowAnimatedText(label);
    }

    private void ShowAnimatedText(string message)
    {
        if (feedbackText == null)
            return;

        if (activeTextRoutine != null)
        {
            StopCoroutine(activeTextRoutine);
        }

        activeTextRoutine = StartCoroutine(AnimateFeedbackText(message));
    }

    private IEnumerator AnimateFeedbackText(string message)
    {
        feedbackText.text = message;

        if (feedbackTextPoint != null)
        {
            feedbackText.transform.position = feedbackTextPoint.position;
            feedbackText.transform.rotation = feedbackTextPoint.rotation;
        }

        feedbackText.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < textLifeTime)
        {
            timer += Time.deltaTime;

            float normalized = Mathf.Clamp01(timer / textLifeTime);

            float scale;

            if (normalized < 0.35f)
            {
                float t = normalized / 0.35f;
                scale = Mathf.Lerp(startScale, peakScale, t);
            }
            else
            {
                float t = (normalized - 0.35f) / 0.65f;
                scale = Mathf.Lerp(peakScale, endScale, t);
            }

            feedbackText.transform.localScale = Vector3.one * scale;

            if (Camera.main != null)
            {
                feedbackText.transform.LookAt(
                    feedbackText.transform.position + Camera.main.transform.rotation * Vector3.forward,
                    Camera.main.transform.rotation * Vector3.up
                );
            }

            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        activeTextRoutine = null;
    }
}