namespace Doomlike.UI
{
    public class OptionHeadBob : OptionRaycasterToggle
    {
        public override void Init()
        {
            OnToggleValueChanged(_toggle.isOn);
        }

        public override void OnToggleValueChanged(bool value)
        {
            Manager.ReferencesHub.FPSMaster.FPSHeadBob.SetState(_toggle.isOn);
        }
    }
}