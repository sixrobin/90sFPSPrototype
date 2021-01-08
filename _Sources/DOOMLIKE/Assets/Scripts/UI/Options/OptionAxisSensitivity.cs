namespace Doomlike.UI
{
    using UnityEngine;

    public class OptionAxisSensitivity : OptionRaycasterValues
    {
        [SerializeField] private FPSCtrl.FPSCamera _fpsCamera = null;
        [SerializeField] private FPSCtrl.FPSCamera.Axis _axis = FPSCtrl.FPSCamera.Axis.None;

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
}