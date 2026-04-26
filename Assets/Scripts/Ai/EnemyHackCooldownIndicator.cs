using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHackCooldownIndicator : MonoBehaviour
{
    private const string IndicatorName = "HackCooldownIndicator";
    private const string FullCircle = "\u25CF";
    private const string MostlyFullCircle = "\u25D5";
    private const string HalfCircle = "\u25D1";
    private const string QuarterCircle = "\u25D4";
    private const string EmptyCircle = "\u25CB";

    [SerializeField] private TextMesh cooldownText;
    [SerializeField] private Vector3 cooldownOffset = new Vector3(0f, 1.75f, 0f);
    [SerializeField] private Color cooldownColor = new Color(0.3f, 1f, 1f, 0.95f);
    [SerializeField] private int sortingOrder = 2002;

    private Camera _mainCamera;

    public void Configure(Vector3 offset, bool allowCreate)
    {
        cooldownOffset = offset;
        EnsureCooldownText(allowCreate);
        ClearCooldownText();
    }

    public void ShowCooldown(float normalizedRemaining)
    {
        EnsureCooldownText(allowCreate: true);
        if (cooldownText == null)
        {
            return;
        }

        float clampedRemaining = Mathf.Clamp01(normalizedRemaining);
        if (clampedRemaining <= 0f)
        {
            HideCooldown();
            return;
        }

        cooldownText.text = ResolveCooldownGlyph(clampedRemaining);
        cooldownText.color = cooldownColor;
        cooldownText.gameObject.SetActive(true);
    }

    public void HideCooldown()
    {
        if (cooldownText == null)
        {
            return;
        }

        ClearCooldownText();
        cooldownText.gameObject.SetActive(false);
    }

    public void RefreshPresentation()
    {
        if (cooldownText == null)
        {
            return;
        }

        cooldownText.transform.localPosition = cooldownOffset;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            cooldownText.transform.rotation = _mainCamera.transform.rotation;
        }
    }

    private void EnsureCooldownText(bool allowCreate)
    {
        if (cooldownText != null)
        {
            return;
        }

        Transform existing = transform.Find(IndicatorName);
        if (existing != null)
        {
            cooldownText = existing.GetComponent<TextMesh>();
            if (cooldownText != null)
            {
                return;
            }
        }

        if (!allowCreate)
        {
            return;
        }

        GameObject indicator = existing != null ? existing.gameObject : new GameObject(IndicatorName);
        if (existing == null)
        {
            indicator.transform.SetParent(transform, false);
        }

        indicator.transform.localPosition = cooldownOffset;

        LineRenderer legacyRenderer = indicator.GetComponent<LineRenderer>();
        if (legacyRenderer != null)
        {
            legacyRenderer.enabled = false;
        }

        cooldownText = indicator.GetComponent<TextMesh>();
        if (cooldownText == null)
        {
            cooldownText = indicator.AddComponent<TextMesh>();
        }

        cooldownText.fontSize = 72;
        cooldownText.characterSize = 0.08f;
        cooldownText.anchor = TextAnchor.MiddleCenter;
        cooldownText.alignment = TextAlignment.Center;
        cooldownText.color = cooldownColor;
        ClearCooldownText();

        MeshRenderer meshRenderer = cooldownText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }
    }

    private static string ResolveCooldownGlyph(float normalizedRemaining)
    {
        if (normalizedRemaining > 0.8f)
        {
            return FullCircle;
        }

        if (normalizedRemaining > 0.6f)
        {
            return MostlyFullCircle;
        }

        if (normalizedRemaining > 0.4f)
        {
            return HalfCircle;
        }

        if (normalizedRemaining > 0.2f)
        {
            return QuarterCircle;
        }

        return EmptyCircle;
    }

    private void ClearCooldownText()
    {
        if (cooldownText != null)
        {
            cooldownText.text = string.Empty;
        }
    }
}
