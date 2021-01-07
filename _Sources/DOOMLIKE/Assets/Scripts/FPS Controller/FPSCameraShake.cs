﻿using UnityEngine;

public class FPSCameraShake : FPSCameraExtraMovement
{
    [SerializeField] private Shake.ShakeSettings _settings = Shake.ShakeSettings.Default;

    private Shake _shake = null;

    public override void ApplyMovement()
    {
        ApplyShake();
    }

    public void AddTrauma(float value)
    {
        _shake.AddTrauma(value);
    }

    public void SetTrauma(float value)
    {
        _shake.SetTrauma(value);
    }

    public void ResetTrauma()
    {
        _shake.SetTrauma(0f);
    }

    public void SetShakePercentage(float value)
    {
        _shake.SetMultiplier(value * 0.01f);
    }

    private void ApplyShake()
    {
        System.Tuple<Vector3, Quaternion> evaluatedShake = _shake.Evaluate(transform);
        if (evaluatedShake == null)
            return;

        transform.position += evaluatedShake.Item1;
        transform.rotation *= evaluatedShake.Item2;
    }

    private void Awake()
    {
        _shake = new Shake(_settings);
    }
}