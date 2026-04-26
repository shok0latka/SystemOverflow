using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    public float moveSpeed = 5f;
    
    [Header("Smoothing")]
    [Tooltip("Как быстро персонаж набирает скорость (больше = резче)")]
    public float acceleration = 20f;
    [Tooltip("Как быстро персонаж останавливается (больше = резче)")]
    public float deceleration = 40f;

    private float _baseSpeed;
    private Rigidbody2D _rb;
    private Vector2 _input;
    private Vector2 _currentVelocity;

    private bool _isCrouching;
    private bool _isSpeed;

    private void Awake()
    {
        _baseSpeed = moveSpeed;
        ConfigureTopDownRigidbody();
    }

    private void Reset()
    {
        ConfigureTopDownRigidbody();
    }

    private void OnValidate()
    {
        ConfigureTopDownRigidbody();
    }

    private void Update()
    {
        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        if (_rb == null)
        {
            ConfigureTopDownRigidbody();
        }

        if (_rb == null)
        {
            return;
        }

        Vector2 targetVelocity = _input * moveSpeed;

        float smoothFactor = _input.magnitude > 0.01f ? acceleration : deceleration;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            targetVelocity,
            smoothFactor * Time.fixedDeltaTime
        );

        _rb.velocity = _currentVelocity;
    }

    private void ConfigureTopDownRigidbody()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_rb == null)
        {
            return;
        }

        _rb.gravityScale = 0f;
        _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    public void SpeedUp(bool isSpeed)
    {
        if (!_isCrouching)
        {
            _isSpeed = isSpeed;
            moveSpeed = _isSpeed ? _baseSpeed * 2f : _baseSpeed;
        }
    }

    public void Crouch(bool isCrouching)
    {
        if (!_isSpeed)
        {
            _isCrouching = isCrouching;
            moveSpeed = _isCrouching ? _baseSpeed * 0.5f : _baseSpeed;
        }
    }
    public Vector2 CurrentVelocity => _currentVelocity;
}