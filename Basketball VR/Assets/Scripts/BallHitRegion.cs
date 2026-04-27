using UnityEngine;

public class BallHitRegion : MonoBehaviour
{
    public enum SurfaceType
    {
        Rim,
        Backboard,
        Floor,
        Other
    }

    [Header("Bölge Tipi")]
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Rim;

    [Header("UI Ýçin Görünen Ýsim")]
    [SerializeField] private string displayName = "Çember Bölge 01";

    [Header("CSV Ýçin Yazýlacak Ýsim")]
    [SerializeField] private string csvRegionName = "Rim_01";

    [Header("Ýsteðe Baðlý: Hit Object Override")]
    [SerializeField] private string hitObjectOverride = "";

    public SurfaceType Type => surfaceType;

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return gameObject.name;

            return displayName;
        }
    }

    public string CsvRegionName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(csvRegionName))
                return gameObject.name.Replace(" ", "_");

            return csvRegionName;
        }
    }

    public string HitObject
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(hitObjectOverride))
                return hitObjectOverride;

            switch (surfaceType)
            {
                case SurfaceType.Rim:
                    return "Pota Çemberi";

                case SurfaceType.Backboard:
                    return "Panya";

                case SurfaceType.Floor:
                    return "Zemin";

                default:
                    return "Diðer";
            }
        }
    }
}