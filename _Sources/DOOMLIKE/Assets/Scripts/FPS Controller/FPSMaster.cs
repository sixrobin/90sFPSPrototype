namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    /// <summary>
    /// Main class for the FPS controller. Controls the more generic things of the controller.
    /// </summary>
    public class FPSMaster : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private FPSControllableComponent[] _allComponents = null;
        [SerializeField] private FPSController _fpsController = null;
        [SerializeField] private FPSCameraShake _fpsCameraShake = null;
        
        [SerializeField] private Manager.OptionsManager _optionsManager = null;
        [SerializeField] private UI.TrainingWorkshopTerminalScreen _trainingWorkshopTerminalScreen = null;

        public FPSController FPSController => _fpsController;

        public FPSCameraShake FPSCameraShake => _fpsCameraShake;

        public Manager.OptionsManager OptionsManager => _optionsManager;

        public string ConsoleProPrefix => "FPS Master";

        /// <summary>
        /// Enables all the controllable components of the FPS controller.
        /// </summary>
        [ContextMenu("Enable All Components")]
        public void EnableAllComponents()
        {
            ConsoleProLogger.Log(this, $"Enabling all {_allComponents.Length} components.", gameObject);
            for (int i = _allComponents.Length - 1; i >= 0; --i)
                _allComponents[i].Controllable = true;
        }

        public System.Collections.IEnumerator EnableAllComponentsAtEndOfFrame()
        {
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            EnableAllComponents();
        }

        /// <summary>
        /// Disables all the controllable components of the FPS controller.
        /// </summary>
        [ContextMenu("Disable All Components")]
        public void DisableAllComponents()
        {
            ConsoleProLogger.Log(this, $"Disabling all {_allComponents.Length} components.", gameObject);
            for (int i = _allComponents.Length - 1; i >= 0; --i)
                _allComponents[i].Controllable = false;
        }

        private void OnTerminalScreenToggled(bool state)
        {
            if (state)
                DisableAllComponents();
            else
                StartCoroutine(EnableAllComponentsAtEndOfFrame());
        }

        private void Awake()
        {
            for (int i = _allComponents.Length - 1; i >= 0; --i)
                _allComponents[i].SetFPSMaster(this);

            _trainingWorkshopTerminalScreen.TerminalScreenToggled += OnTerminalScreenToggled;
        }

        private void OnDestroy()
        {
            _trainingWorkshopTerminalScreen.TerminalScreenToggled -= OnTerminalScreenToggled;
        }
    }
}