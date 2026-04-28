using UnityEngine;

public class SuspicionMeter
{
    private float _increasePerSecond;
    private float _decreasePerSecond;

    public SuspicionMeter(float increasePerSecond, float decreasePerSecond)
    {
        Configure(increasePerSecond, decreasePerSecond);
    }

    public float Value { get; private set; }

    public void Configure(float increasePerSecond, float decreasePerSecond)
    {
        _increasePerSecond = Mathf.Max(0f, increasePerSecond);
        _decreasePerSecond = Mathf.Max(0f, decreasePerSecond);
    }

    public void Tick(bool detected, float deltaTime)
    {
        float safeDeltaTime = Mathf.Max(0f, deltaTime);

        if (detected)
        {
            Value += _increasePerSecond * safeDeltaTime;
        }
        else
        {
            Value -= _decreasePerSecond * safeDeltaTime;
        }

        Value = Mathf.Clamp01(Value);
    }

    public bool IsTriggered(float threshold)
    {
        return Value >= Mathf.Clamp01(threshold);
    }

    public void Set(float value)
    {
        Value = Mathf.Clamp01(value);
    }

    public void Reset()
    {
        Value = 0f;
    }
}
