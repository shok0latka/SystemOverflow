using UnityEngine;

public sealed class EnemyAlertSignal : MonoBehaviour
{
    private const string SignalName = "EnemyAlertSignal";
    private const float ArrivalDistance = 0.05f;
    private const float MinimumSpeed = 0.05f;
    private static readonly Vector3 VisualOffset = new Vector3(0f, 1.9f, 0f);

    private EnemyAI2D _receiver;
    private Vector2 _alertPosition;
    private float _speed;
    private TextMesh _signalText;
    private Camera _mainCamera;

    public static void Spawn(
        Vector2 startPosition,
        EnemyAI2D receiver,
        Vector2 alertPosition,
        float speed)
    {
        if (receiver == null)
        {
            return;
        }

        GameObject signalObject = new GameObject(SignalName);
        signalObject.transform.position = (Vector3)startPosition + VisualOffset;

        EnemyAlertSignal signal = signalObject.AddComponent<EnemyAlertSignal>();
        signal.Initialize(receiver, alertPosition, speed);
    }

    private void Initialize(EnemyAI2D receiver, Vector2 alertPosition, float speed)
    {
        _receiver = receiver;
        _alertPosition = alertPosition;
        _speed = Mathf.Max(MinimumSpeed, speed);
        EnsureSignalText();
    }

    private void Update()
    {
        if (_receiver == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = _receiver.transform.position + VisualOffset;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            _speed * Time.deltaTime);

        RefreshPresentation();

        if ((transform.position - targetPosition).sqrMagnitude > ArrivalDistance * ArrivalDistance)
        {
            return;
        }

        _receiver.ReceiveEnemyAlert(_alertPosition);
        Destroy(gameObject);
    }

    private void EnsureSignalText()
    {
        if (_signalText != null)
        {
            return;
        }

        _signalText = gameObject.AddComponent<TextMesh>();
        _signalText.text = "!";
        _signalText.fontSize = 96;
        _signalText.characterSize = 0.12f;
        _signalText.anchor = TextAnchor.MiddleCenter;
        _signalText.alignment = TextAlignment.Center;
        _signalText.color = new Color(0.35f, 0.95f, 1f);

        MeshRenderer meshRenderer = _signalText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 2200;
        }
    }

    private void RefreshPresentation()
    {
        if (_signalText == null)
        {
            return;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            _signalText.transform.rotation = _mainCamera.transform.rotation;
        }
    }
}
