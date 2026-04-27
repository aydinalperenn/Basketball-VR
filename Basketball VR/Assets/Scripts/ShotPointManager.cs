using System;
using UnityEngine;

public class ShotPointManager : MonoBehaviour
{
    public static ShotPointManager Instance { get; private set; }

    [Serializable]
    public class ShotPoint
    {
        public string pointName = "ShotPoint";
        public Transform pointTransform;
        public float activationRadius = 1.5f;
    }

    [Header("Oyuncu Referansý")]
    [Tooltip("Main Camera veya XR Origin Camera ver. Boþ kalýrsa Camera.main kullanýlýr.")]
    [SerializeField] private Transform playerReference;

    [Header("Teleport / Atýþ Noktalarý")]
    [SerializeField] private ShotPoint[] shotPoints;

    [Header("Varsayýlan Deðer")]
    [SerializeField] private string unknownPointName = "UnknownPoint";

    private string currentShotPointName = "UnknownPoint";

    private void Awake()
    {
        Instance = this;

        if (playerReference == null && Camera.main != null)
        {
            playerReference = Camera.main.transform;
        }

        currentShotPointName = unknownPointName;
    }

    private void Update()
    {
        UpdateCurrentShotPoint();
    }

    public string GetCurrentShotPointName()
    {
        if (string.IsNullOrWhiteSpace(currentShotPointName))
            return unknownPointName;

        return currentShotPointName;
    }

    private void UpdateCurrentShotPoint()
    {
        if (playerReference == null || shotPoints == null || shotPoints.Length == 0)
        {
            currentShotPointName = unknownPointName;
            return;
        }

        float bestDistanceSqr = float.MaxValue;
        string bestName = unknownPointName;
        bool foundPoint = false;

        for (int i = 0; i < shotPoints.Length; i++)
        {
            ShotPoint point = shotPoints[i];

            if (point == null || point.pointTransform == null)
                continue;

            float radius = Mathf.Max(0.1f, point.activationRadius);
            float distanceSqr = (playerReference.position - point.pointTransform.position).sqrMagnitude;

            if (distanceSqr <= radius * radius && distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestName = string.IsNullOrWhiteSpace(point.pointName)
                    ? point.pointTransform.name
                    : point.pointName;

                foundPoint = true;
            }
        }

        currentShotPointName = foundPoint ? bestName : unknownPointName;
    }
}