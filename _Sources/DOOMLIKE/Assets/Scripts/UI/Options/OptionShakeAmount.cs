using UnityEngine;

public class OptionShakeAmount : OptionRaycasterValues
{
    [SerializeField] private FPSCameraShake _fpsCameraShake = null;

    public override void Init()
    {
    }

    public override string FormatValueDisplay(string display)
    {
        return $"{display}<b>%</b>";
    }

    public override void OnValueChanged(string value)
    {
        if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float shakeMult))
        {
            Debug.LogError($"Could not parse {value} to a float value!");
            return;
        }

        _fpsCameraShake.SetShakePercentage(shakeMult);
    }
}