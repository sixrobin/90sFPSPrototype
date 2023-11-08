namespace Doomlike.UI
{
    using UnityEngine;

    public abstract class OptionRaycasterToggle : OptionRaycaster
    {
        [SerializeField] protected UnityEngine.UI.Toggle _toggle = null;

        public override void OnClicked()
        {
            _toggle.isOn = !_toggle.isOn;
        }

        public abstract void OnToggleValueChanged(bool value);

        protected override void Start()
        {
            base.Start();
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }
}