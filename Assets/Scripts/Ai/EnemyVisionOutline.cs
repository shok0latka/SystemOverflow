using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class EnemyVisionOutline : MonoBehaviour
{
    private const int ArcSegments = 12;
    private const int RingSegments = 24;
    private const string CloseRangeRendererChildName = "EnemyVisionCloseRange";

    [FormerlySerializedAs("lineRenderer")]
    [SerializeField] private LineRenderer coneLineRenderer;
    [SerializeField] private LineRenderer closeRangeLineRenderer;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = new Color(1f, 0.82f, 0.2f, 0.95f);
    [SerializeField] private int sortingOrder = 1500;

    private static Material _sharedMaterial;

    public void RefreshOutline(
        Vector3 origin,
        Vector2 viewDirection,
        float visionRadius,
        float closeVisionRadius,
        float coneAngleDegrees,
        bool shouldShow,
        bool allowCreate)
    {
        float safeConeAngleDegrees = Mathf.Clamp(coneAngleDegrees, 1f, 360f);
        bool canRenderCone = shouldShow && visionRadius > 0.05f && viewDirection.sqrMagnitude > 0.0001f;
        bool canRenderCloseRange = shouldShow && closeVisionRadius > 0.05f;

        LineRenderer targetConeLineRenderer = EnsureConeLineRenderer(allowCreate);
        LineRenderer targetCloseRangeLineRenderer = EnsureCloseRangeLineRenderer(allowCreate);

        if (targetConeLineRenderer == null && targetCloseRangeLineRenderer == null)
        {
            return;
        }

        if (targetConeLineRenderer != null)
        {
            targetConeLineRenderer.enabled = canRenderCone;
        }

        if (targetCloseRangeLineRenderer != null)
        {
            targetCloseRangeLineRenderer.enabled = canRenderCloseRange;
        }

        if (!canRenderCone && !canRenderCloseRange)
        {
            return;
        }

        if (canRenderCone && targetConeLineRenderer != null)
        {
            ConfigureLineRenderer(targetConeLineRenderer, loop: false, lineColor);

            Vector3[] conePositions = BuildConePositions(origin, viewDirection.normalized, visionRadius, safeConeAngleDegrees);
            targetConeLineRenderer.positionCount = conePositions.Length;
            targetConeLineRenderer.SetPositions(conePositions);
        }

        if (canRenderCloseRange && targetCloseRangeLineRenderer != null)
        {
            Color closeRangeColor = lineColor;
            closeRangeColor.a *= 0.8f;
            ConfigureLineRenderer(targetCloseRangeLineRenderer, loop: true, closeRangeColor);

            Vector3[] closeRangePositions = BuildRingPositions(origin, closeVisionRadius);
            targetCloseRangeLineRenderer.positionCount = closeRangePositions.Length;
            targetCloseRangeLineRenderer.SetPositions(closeRangePositions);
        }
    }

    private LineRenderer EnsureConeLineRenderer(bool allowCreate)
    {
        if (coneLineRenderer != null)
        {
            return coneLineRenderer;
        }

        LineRenderer[] lineRenderers = GetComponents<LineRenderer>();
        if (lineRenderers.Length > 0)
        {
            coneLineRenderer = lineRenderers[0];
        }

        if (coneLineRenderer == null && allowCreate)
        {
            coneLineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        return coneLineRenderer;
    }

    private LineRenderer EnsureCloseRangeLineRenderer(bool allowCreate)
    {
        if (IsValidCloseRangeLineRenderer(closeRangeLineRenderer))
        {
            return closeRangeLineRenderer;
        }

        Transform closeRangeChild = transform.Find(CloseRangeRendererChildName);
        if (closeRangeChild != null)
        {
            closeRangeLineRenderer = closeRangeChild.GetComponent<LineRenderer>();
        }

        if (IsValidCloseRangeLineRenderer(closeRangeLineRenderer))
        {
            return closeRangeLineRenderer;
        }

        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            if (IsValidCloseRangeLineRenderer(lineRenderer))
            {
                closeRangeLineRenderer = lineRenderer;
                return closeRangeLineRenderer;
            }
        }

        if (!allowCreate)
        {
            return null;
        }

        GameObject childObject = closeRangeChild != null
            ? closeRangeChild.gameObject
            : new GameObject(CloseRangeRendererChildName);

        if (closeRangeChild == null)
        {
            childObject.transform.SetParent(transform, false);
        }

        closeRangeLineRenderer = childObject.GetComponent<LineRenderer>();
        if (closeRangeLineRenderer == null)
        {
            closeRangeLineRenderer = childObject.AddComponent<LineRenderer>();
        }

        return closeRangeLineRenderer;
    }

    private bool IsValidCloseRangeLineRenderer(LineRenderer lineRenderer)
    {
        return lineRenderer != null && lineRenderer != coneLineRenderer;
    }

    private void ConfigureLineRenderer(LineRenderer targetLineRenderer, bool loop, Color color)
    {
        targetLineRenderer.useWorldSpace = true;
        targetLineRenderer.loop = loop;
        targetLineRenderer.alignment = LineAlignment.View;
        targetLineRenderer.startWidth = lineWidth;
        targetLineRenderer.endWidth = lineWidth;
        targetLineRenderer.startColor = color;
        targetLineRenderer.endColor = color;
        targetLineRenderer.sortingOrder = sortingOrder;

        if (targetLineRenderer.sharedMaterial == null)
        {
            Material material = GetSharedMaterial();
            if (material != null)
            {
                targetLineRenderer.sharedMaterial = material;
            }
        }
    }

    private static Vector3[] BuildConePositions(
        Vector3 origin,
        Vector2 viewDirection,
        float radius,
        float coneAngleDegrees)
    {
        int arcPointCount = ArcSegments + 1;
        Vector3[] positions = new Vector3[arcPointCount + 2];
        positions[0] = origin;

        float baseAngle = Mathf.Atan2(viewDirection.y, viewDirection.x) * Mathf.Rad2Deg;
        float halfConeAngleDegrees = coneAngleDegrees * 0.5f;
        for (int i = 0; i < arcPointCount; i++)
        {
            float interpolation = i / (float)(arcPointCount - 1);
            float angle = baseAngle - halfConeAngleDegrees +
                interpolation * coneAngleDegrees;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            positions[i + 1] = origin + (Vector3)(direction * radius);
        }

        positions[positions.Length - 1] = origin;
        return positions;
    }

    private static Vector3[] BuildRingPositions(Vector3 origin, float radius)
    {
        Vector3[] positions = new Vector3[RingSegments];
        for (int i = 0; i < RingSegments; i++)
        {
            float interpolation = i / (float)RingSegments;
            float radians = interpolation * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            positions[i] = origin + (Vector3)(direction * radius);
        }

        return positions;
    }

    private static Material GetSharedMaterial()
    {
        if (_sharedMaterial != null)
        {
            return _sharedMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            return null;
        }

        _sharedMaterial = new Material(shader)
        {
            name = "EnemyVisionOutlineMaterial"
        };

        return _sharedMaterial;
    }
}
