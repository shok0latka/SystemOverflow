using UnityEngine;

[DisallowMultipleComponent]
public class EnemyStatusIndicator : MonoBehaviour
{
    private const string IndicatorName = "StateIndicator";

    [SerializeField] private TextMesh statusText;
    [SerializeField] private Vector3 statusOffset = new Vector3(0f, 1.4f, 0f);

    private Camera _mainCamera;

    public void Configure(TextMesh existingStatusText, Vector3 offset, bool allowCreate)
    {
        if (existingStatusText != null)
        {
            statusText = existingStatusText;
        }

        statusOffset = offset;
        EnsureStatusText(allowCreate);
    }

    public void ApplyState(EnemyState state, bool allowCreate = true)
    {
        EnsureStatusText(allowCreate);
        if (statusText == null)
        {
            return;
        }

        switch (state)
        {
            case EnemyState.Patrol:
                statusText.text = "P";
                statusText.color = new Color(0.55f, 0.95f, 0.55f);
                break;
            case EnemyState.Chase:
                statusText.text = "C";
                statusText.color = new Color(1f, 0.9f, 0.3f);
                break;
            case EnemyState.Attack:
                statusText.text = "A";
                statusText.color = new Color(1f, 0.35f, 0.35f);
                break;
            case EnemyState.Hacked:
                statusText.text = "H";
                statusText.color = new Color(0.8f, 0.55f, 1f);
                break;
            case EnemyState.Search:
                statusText.text = "S";
                statusText.color = new Color(0.45f, 0.95f, 1f);
                break;
        }
    }

    public void RefreshPresentation()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.transform.localPosition = statusOffset;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            statusText.transform.rotation = _mainCamera.transform.rotation;
        }
    }

    private void EnsureStatusText(bool allowCreate)
    {
        if (statusText != null)
        {
            return;
        }

        Transform existing = transform.Find(IndicatorName);
        if (existing != null)
        {
            statusText = existing.GetComponent<TextMesh>();
            if (statusText != null)
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
        indicator.transform.localPosition = statusOffset;

        statusText = indicator.AddComponent<TextMesh>();
        statusText.fontSize = 72;
        statusText.characterSize = 0.08f;
        statusText.anchor = TextAnchor.MiddleCenter;
        statusText.alignment = TextAlignment.Center;

        MeshRenderer meshRenderer = statusText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 2000;
        }
    }
}
