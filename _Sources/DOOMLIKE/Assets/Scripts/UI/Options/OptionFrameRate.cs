namespace Doomlike.UI
{
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
                ConsoleProLogger.LogMisc("Setting Application target frame rate to unclamped.");

                Application.targetFrameRate = -1;
                return;
            }

            if (!int.TryParse(value, out int fps))
            {
                Debug.LogError($"Could not parse {value} to an integer value!");
                return;
            }

            ConsoleProLogger.LogMisc($"Setting Application target frame rate to {fps} fps.");
            Application.targetFrameRate = fps;
        }
    }
}