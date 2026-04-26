using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHackProgressIndicator : MonoBehaviour
{
    private const string IndicatorName = "HackProgressIndicator";
    private const int SegmentCount = 10;

    [SerializeField] private TextMesh progressText;
    [SerializeField] private Vector3 progressOffset = new Vector3(0f, 1.75f, 0f);

    private Camera _mainCamera;

    public void Configure(Vector3 offset, bool allowCreate)
    {
        progressOffset = offset;
        EnsureProgressText(allowCreate);
        ClearProgressText();
    }

    public void ShowProgress(float normalizedProgress)
    {
        EnsureProgressText(allowCreate: true);
        if (progressText == null)
        {
            return;
        }

        int filledSegments = Mathf.RoundToInt(Mathf.Clamp01(normalizedProgress) * SegmentCount);
        progressText.text = $"[{new string('#', filledSegments)}{new string('-', SegmentCount - filledSegments)}]";
        progressText.gameObject.SetActive(true);
    }

    public void HideProgress()
    {
        if (progressText == null)
        {
            return;
        }

        ClearProgressText();
        progressText.gameObject.SetActive(false);
    }

    public void RefreshPresentation()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.transform.localPosition = progressOffset;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            progressText.transform.rotation = _mainCamera.transform.rotation;
        }
    }

    private void EnsureProgressText(bool allowCreate)
    {
        if (progressText != null)
        {
            return;
        }

        Transform existing = transform.Find(IndicatorName);
        if (existing != null)
        {
            progressText = existing.GetComponent<TextMesh>();
            if (progressText != null)
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
        indicator.transform.localPosition = progressOffset;

        progressText = indicator.AddComponent<TextMesh>();
        progressText.fontSize = 48;
        progressText.characterSize = 0.06f;
        progressText.anchor = TextAnchor.MiddleCenter;
        progressText.alignment = TextAlignment.Center;
        progressText.color = new Color(0.3f, 1f, 1f);
        ClearProgressText();

        MeshRenderer meshRenderer = progressText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 2001;
        }
    }

    private void ClearProgressText()
    {
        if (progressText != null)
        {
            progressText.text = string.Empty;
        }
    }
}
