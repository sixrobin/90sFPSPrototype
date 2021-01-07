using UnityEngine;

public class OptionFrameRate : OptionRaycasterValues
{
    public override void Init()
    {
        OnValueChanged(_valueText.text);
    }

    public override void OnValueChanged(string value)
    {
        if (value == "MAX")
        {
            Application.targetFrameRate = -1;
            return;
        }

        if (!int.TryParse(value, out int frameRate))
        {
            Debug.LogError($"Could not parse {value} to an integer value!");
            return;
        }

        Application.targetFrameRate = frameRate;
    }
}