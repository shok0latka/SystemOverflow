using System;
using UnityEngine;


//Сами разберётесь что тут к чему

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private float _baseSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _input;

    private bool _isCrouchng = false;

    private bool _isSpeed = false;

    private void Awake()
    {
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

        _rb.velocity = _input * moveSpeed;
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
        if(!_isCrouchng)
        {
            _isSpeed = isSpeed;

            if(!_isSpeed)
                moveSpeed = _baseSpeed ;
            else
                moveSpeed = _baseSpeed * 2.0f;
            Debug.Log(moveSpeed);  
        }
    }

    public void Crouch(bool isCrouchng)
    {
        if(!_isSpeed)
        {
            _isCrouchng = isCrouchng;

            if(!_isCrouchng)
                moveSpeed = _baseSpeed;
            else
                moveSpeed = _baseSpeed / 2.0f;

            Debug.Log(moveSpeed);   
        }
    }
}
