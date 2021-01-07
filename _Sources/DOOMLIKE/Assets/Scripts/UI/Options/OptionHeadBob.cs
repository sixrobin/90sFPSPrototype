﻿using UnityEngine;

public class OptionHeadBob : OptionRaycasterToggle
{
    [SerializeField] private FPSHeadBob _fpsHeadBob = null;

    public override void Init()
    {
        OnToggleValueChanged(_toggle.isOn);
    }

    public override void OnToggleValueChanged(bool value)
    {
        _fpsHeadBob.SetState(_toggle.isOn);
    }
}