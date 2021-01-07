using UnityEngine;

public class OptionReverseAxis : OptionRaycasterToggle
{
    [SerializeField] private FPSCamera.Axis _axis = FPSCamera.Axis.None;
    [SerializeField] private FPSCamera _fpsCamera = null;

    public override void Init()
    {
    }

    public override void OnToggleValueChanged(bool value)
    {
        if (_axis == FPSCamera.Axis.None)
        {
            Debug.LogError("Invalid axis.");
            return;
        }

        _fpsCamera.ReverseAxis(_axis);
    }
}