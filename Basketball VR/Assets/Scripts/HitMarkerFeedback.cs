using TMPro;
using UnityEngine;

public class HitMarkerFeedback : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private float markerLifeTime = 2f;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.03f, 0f);

    public void ShowMarker(Vector3 hitPoint, string label)
    {
        if (markerPrefab == null)
            return;

        GameObject marker = Instantiate(markerPrefab, hitPoint + spawnOffset, Quaternion.identity);

        TMP_Text text = marker.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
        }

        Destroy(marker, markerLifeTime);
    }
}