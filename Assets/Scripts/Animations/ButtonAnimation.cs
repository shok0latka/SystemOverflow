using System;
using UnityEngine;

public class ButtonAnimationY : MonoBehaviour {
    
    [SerializeField] float speed = 2f;
    [SerializeField] float amplitude = 0.5f;

    Vector3 baseScale;

    private void Start() {
        baseScale = transform.localScale;
    }

    void Update() {
        float delta = 1 + Math.Abs(amplitude * Mathf.Sin(Time.time * speed));
        transform.localScale = new (baseScale.x * delta, baseScale.y * delta, baseScale.z);
    }
}
