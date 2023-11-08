namespace Doomlike.Manager
{
    using UnityEngine;

    public class OptionsManager : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private bool _logsMuted = false;
        [SerializeField] private GameObject _optionsView = null;

        public delegate void OptionsStateChangedEventHandler(bool state);

        public event OptionsStateChangedEventHandler OptionsStateChanged;

        public bool IsOpen { get; private set; } = false;

        public string ConsoleProPrefix => "Options Manager";

        public bool ConsoleProMuted => _logsMuted;

        private void ToggleView()
        {
            IsOpen = !IsOpen;

            ConsoleProLogger.Log(this, IsOpen ? "Opening options." : "Closing options.", gameObject);

            _optionsView.SetActive(IsOpen);

            if (IsOpen)
                ReferencesHub.TimeManager.Freeze();
            else
                ReferencesHub.TimeManager.Unfreeze();

            OptionsStateChanged(IsOpen);
        }

        private void Start()
        {
            _optionsView.SetActive(IsOpen);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleView();
        }
    }
}