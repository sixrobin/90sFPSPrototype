namespace Doomlike.Manager
{
    using UnityEngine;

    public class CursorManager : MonoBehaviour, IConsoleProLoggable
    {
        public string ConsoleProPrefix => "Cursor Manager";

        private void OnOptionsStateChanged(bool state)
        {
            SetCursorState(state);
        }

        private void OnDebugConsoleToggled(bool state)
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
            ReferencesHub.OptionsManager.OptionsStateChanged += OnOptionsStateChanged;
            Console.DebugConsole.Instance.DebugConsoleToggled += OnDebugConsoleToggled;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void OnDestroy()
        {
            if (ReferencesHub.Exists())
                ReferencesHub.OptionsManager.OptionsStateChanged -= OnOptionsStateChanged;

            if (Console.DebugConsole.Exists())
                Console.DebugConsole.Instance.DebugConsoleToggled -= OnDebugConsoleToggled;
        }
    }
}