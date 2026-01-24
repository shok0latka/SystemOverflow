using System;
using UnityEngine;


//Сами разберётесь что тут к чему

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private float _baseSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    private bool _isCrouchng = false;

    private bool _isSpeed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        rb.velocity = input * moveSpeed;
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
