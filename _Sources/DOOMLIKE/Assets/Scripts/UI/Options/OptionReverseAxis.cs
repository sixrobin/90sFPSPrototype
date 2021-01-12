namespace Doomlike.UI
{
    using UnityEngine;

    public class OptionReverseAxis : OptionRaycasterToggle
    {
        [SerializeField] private FPSCtrl.FPSCamera.Axis _axis = FPSCtrl.FPSCamera.Axis.None;

        public override void Init()
        {
        }

        public override void OnToggleValueChanged(bool value)
        {
            if (_axis == FPSCtrl.FPSCamera.Axis.None)
            {
                Debug.LogError("Invalid axis.");
                return;
            }

            Manager.ReferencesHub.FPSMaster.FPSCamera.ReverseAxis(_axis);
        }
    }
}