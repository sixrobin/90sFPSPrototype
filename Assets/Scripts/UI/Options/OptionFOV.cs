namespace Doomlike.UI
{
    using UnityEngine;

    public class OptionFOV : OptionRaycasterValues
    {
        [SerializeField] private Camera[] _cameras = null;

        public override void Init()
        {
        }

        public override void OnValueChanged(string value)
        {
            if (!int.TryParse(value, out int fov))
            {
                Debug.LogError($"Could not parse {value} to an int value!");
                return;
            }

            ConsoleProLogger.LogMisc($"Setting {_cameras.Length} cameras FOV to {fov}.");
            for (int i = _cameras.Length - 1; i >= 0; --i)
                _cameras[i].fieldOfView = fov;
        }
    }
}