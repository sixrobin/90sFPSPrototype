namespace Doomlike.Manager
{
    using UnityEngine;

    public class DebugManager : RSLib.Framework.Singleton<DebugManager>, IConsoleProLoggable
    {
        [SerializeField] private bool _logsMuted = false;
        [SerializeField] private bool _debugViewOn = false;

        private GUIStyle _worldSpaceDebugStyle;

        public static bool DebugViewOn => Exists() && Instance._debugViewOn;

        public static GUIStyle WorldSpaceDebugStyle => Instance._worldSpaceDebugStyle;

        public string ConsoleProPrefix => "Debug Manager";

        public bool ConsoleProMuted => _logsMuted;

        protected override void Awake()
        {
            base.Awake();

            this._worldSpaceDebugStyle = new GUIStyle()
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
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("completeWorkshops", "Completes all training workshops.", true, false, DBG_AutoCompleteWorkshops));
        }

        [ContextMenu("Toggle Debug View")]
        private void DBG_ToggleDebugView()
        {
            this._debugViewOn = !this._debugViewOn;
            ConsoleProLogger.Log(this, $"Debug View {(this._debugViewOn ? "on" : "off")}.", gameObject);
        }

        [ContextMenu("Auto Complete Workshops")]
        private void DBG_AutoCompleteWorkshops()
        {
            TrainingWorkshop[] workshops = FindObjectsOfType<TrainingWorkshop>();
            ConsoleProLogger.Log(this, $"Auto completing {workshops.Length} training workshops.", gameObject);
            foreach (TrainingWorkshop workshop in workshops)
                workshop.DBG_AutoComplete();
        }
    }
}