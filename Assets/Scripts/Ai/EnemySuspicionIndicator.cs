using UnityEngine;

[DisallowMultipleComponent]
public class EnemySuspicionIndicator : MonoBehaviour
{
    private const string IndicatorName = "SuspicionIndicator";
    private const int SegmentCount = 10;
    private const float VisibleSuspicionEpsilon = 0.001f;

    private static readonly Color _lowSuspicionColor = new Color(1f, 0.9f, 0.25f);
    private static readonly Color _mediumSuspicionColor = new Color(1f, 0.55f, 0.15f);
    private static readonly Color _highSuspicionColor = new Color(1f, 0.25f, 0.25f);

    [SerializeField] private TextMesh suspicionText;
    [SerializeField] private Vector3 suspicionOffset = new Vector3(0f, 1.65f, 0f);
    [SerializeField] private int sortingOrder = 2003;

    private Camera _mainCamera;

    public void Configure(TextMesh existingSuspicionText, Vector3 offset, bool allowCreate)
    {
        if (existingSuspicionText != null)
        {
            suspicionText = existingSuspicionText;
        }

        suspicionOffset = offset;
        EnsureSuspicionText(allowCreate);
        HideSuspicion();
    }

    public void RefreshSuspicion(float suspicionValue, float suspicionThreshold, bool shouldShow, bool allowCreate)
    {
        EnsureSuspicionText(allowCreate);
        if (suspicionText == null)
        {
            return;
        }

        float normalizedSuspicion = NormalizeSuspicion(suspicionValue, suspicionThreshold);
        if (!shouldShow || normalizedSuspicion <= VisibleSuspicionEpsilon)
        {
            HideSuspicion();
            return;
        }

        int filledSegments = Mathf.Clamp(
            Mathf.CeilToInt(normalizedSuspicion * SegmentCount),
            1,
            SegmentCount);
        suspicionText.text = $"[{new string('#', filledSegments)}{new string('-', SegmentCount - filledSegments)}]";
        suspicionText.color = ResolveSuspicionColor(normalizedSuspicion);
        suspicionText.gameObject.SetActive(true);
    }

    public void RefreshPresentation()
    {
        if (suspicionText == null)
        {
            return;
        }

        suspicionText.transform.localPosition = suspicionOffset;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            suspicionText.transform.rotation = _mainCamera.transform.rotation;
        }
    }

    private void EnsureSuspicionText(bool allowCreate)
    {
        if (suspicionText != null)
        {
            return;
        }

        Transform existing = transform.Find(IndicatorName);
        if (existing != null)
        {
            suspicionText = existing.GetComponent<TextMesh>();
            if (suspicionText != null)
            {
                return;
            }
        }

        if (!allowCreate)
        {
            return;
        }

        GameObject indicator = new GameObject(IndicatorName);
        indicator.transform.SetParent(transform, false);
        indicator.transform.localPosition = suspicionOffset;

        suspicionText = indicator.AddComponent<TextMesh>();
        suspicionText.fontSize = 48;
        suspicionText.characterSize = 0.06f;
        suspicionText.anchor = TextAnchor.MiddleCenter;
        suspicionText.alignment = TextAlignment.Center;
        suspicionText.color = _lowSuspicionColor;
        HideSuspicion();

        MeshRenderer meshRenderer = suspicionText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }
    }

    private static float NormalizeSuspicion(float suspicionValue, float suspicionThreshold)
    {
        float safeSuspicion = Mathf.Clamp01(suspicionValue);
        float safeThreshold = Mathf.Clamp01(suspicionThreshold);
        if (safeThreshold <= VisibleSuspicionEpsilon)
        {
            return safeSuspicion > VisibleSuspicionEpsilon ? 1f : 0f;
        }

        return Mathf.Clamp01(safeSuspicion / safeThreshold);
    }

    private static Color ResolveSuspicionColor(float normalizedSuspicion)
    {
        if (normalizedSuspicion >= 0.85f)
        {
            return _highSuspicionColor;
        }

        if (normalizedSuspicion >= 0.5f)
        {
            return _mediumSuspicionColor;
        }

        return _lowSuspicionColor;
    }

    private void HideSuspicion()
    {
        if (suspicionText == null)
        {
            return;
        }

        suspicionText.text = string.Empty;
        suspicionText.gameObject.SetActive(false);
    }
}
