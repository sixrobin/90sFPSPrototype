namespace Doomlike.Manager
{
    using UnityEngine;

    public class DebugManager : RSLib.Framework.Singleton<DebugManager>, IConsoleProLoggable
    {
        [SerializeField] private bool _logsMuted = false;
        [SerializeField] private bool _dbgViewOn = false;

        private GUIStyle _worldSpaceDbgStyle;

        public static bool DbgViewOn => DebugManager.Exists() ? Instance._dbgViewOn : false;

        public static GUIStyle WorldSpaceDbgStyle => Instance._worldSpaceDbgStyle;

        public string ConsoleProPrefix => "Debug Manager";

        public bool ConsoleProMuted => _logsMuted;

        protected override void Awake()
        {
            base.Awake();

            _worldSpaceDbgStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, 1f)
                }
            };

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("toggleDebugView", "Toggles debug view on screen.", true, false, DBG_ToggleDebugView));
        }

        [ContextMenu("Toggle Debug View")]
        private void DBG_ToggleDebugView()
        {
            _dbgViewOn = !_dbgViewOn;
            ConsoleProLogger.Log(this, $"Debug View {(_dbgViewOn ? "on" : "off")}.", gameObject);
        }
    }
}