using System;
using UnityEngine;

public class ShotPointManager : MonoBehaviour
{
    public static ShotPointManager Instance { get; private set; }

    [Serializable]
    public class ShotPoint
    {
        [Header("Atýþ Noktasý Bilgisi")]
        public string pointName = "ShotPoint";

        [Tooltip("Teleport noktasýnýn transformu.")]
        public Transform pointTransform;

        [Tooltip("Oyuncu bu mesafe içindeyse bu atýþ noktasý aktif sayýlýr.")]
        public float activationRadius = 1.5f;

        [Header("Top Spawn Noktasý")]
        [Tooltip("Oyuncu bu noktaya teleport olunca topun geleceði yer.")]
        public Transform ballSpawnTransform;
    }

    [Header("Oyuncu Referansý")]
    [Tooltip("Main Camera veya XR Origin Camera ver. Boþ kalýrsa Camera.main kullanýlýr.")]
    [SerializeField] private Transform playerReference;

    [Header("Teleport / Atýþ Noktalarý")]
    [SerializeField] private ShotPoint[] shotPoints;

    [Header("Varsayýlan Deðer")]
    [SerializeField] private string unknownPointName = "UnknownPoint";

    private string currentShotPointName = "UnknownPoint";
    private Transform currentBallSpawnTransform;

    private void Awake()
    {
        Instance = this;

        if (playerReference == null && Camera.main != null)
        {
            playerReference = Camera.main.transform;
        }

        currentShotPointName = unknownPointName;
        currentBallSpawnTransform = null;
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

    public Transform GetCurrentBallSpawnTransform()
    {
        return currentBallSpawnTransform;
    }

    private void UpdateCurrentShotPoint()
    {
        if (playerReference == null || shotPoints == null || shotPoints.Length == 0)
        {
            currentShotPointName = unknownPointName;
            currentBallSpawnTransform = null;
            return;
        }

        float bestDistanceSqr = float.MaxValue;
        string bestName = unknownPointName;
        Transform bestBallSpawn = null;
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

                bestBallSpawn = point.ballSpawnTransform;
                foundPoint = true;
            }
        }

        if (foundPoint)
        {
            currentShotPointName = bestName;
            currentBallSpawnTransform = bestBallSpawn;
        }
        else
        {
            currentShotPointName = unknownPointName;
            currentBallSpawnTransform = null;
        }
    }
}