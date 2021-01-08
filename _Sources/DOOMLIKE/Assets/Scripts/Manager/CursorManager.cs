namespace Doomlike.Manager
{
    using UnityEngine;

    public class CursorManager : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private OptionsManager _optionsManager = null;

        public string ConsoleProPrefix => "Cursor Manager";

        private void OnOptionsStateChanged(bool state)
        {
            SetCursorState(state);
        }

        private void SetCursorState(bool state)
        {
            ConsoleProLogger.Log(this, state ? "Showing cursor and unlocking it." : "Hiding cursor and confining it.", gameObject);

            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Confined;
        }

        private void Awake()
        {
            _optionsManager.OptionsStateChanged += OnOptionsStateChanged;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}