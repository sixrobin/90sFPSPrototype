using RSLib.Extensions;
using UnityEngine;

public class OptionCrosshairAlpha : OptionRaycasterValues
{
    [SerializeField] private UnityEngine.UI.Image _crosshair = null;

    public override void Init()
    {
    }

    public override string FormatValueDisplay(string display)
    {
        return $"{display}<b>%</b>";
    }

    public override void OnValueChanged(string value)
    {
        if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float alpha))
        {
            Debug.LogError($"Could not parse {value} to a float value!");
            return;
        }

        _crosshair.color = _crosshair.color.WithA(alpha * 0.01f);
    }
}