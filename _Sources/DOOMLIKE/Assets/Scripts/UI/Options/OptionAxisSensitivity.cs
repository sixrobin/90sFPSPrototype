using UnityEngine;

public class OptionAxisSensitivity : OptionRaycasterValues
{
    [SerializeField] private FPSCamera _fpsCamera = null;
    [SerializeField] private FPSCamera.Axis _axis = FPSCamera.Axis.None;

    public override void Init()
    {
    }

    public override string FormatValueDisplay(string display)
    {
        return $"{display}<b>%</b>";
    }

    public override void OnValueChanged(string value)
    {
        if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float sensitivityMult))
        {
            Debug.LogError($"Could not parse {value} to a float value!");
            return;
        }

        _fpsCamera.SetAxisSensitivityMultiplier(_axis, sensitivityMult);
    }
}