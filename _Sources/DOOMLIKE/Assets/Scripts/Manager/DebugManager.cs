namespace Doomlike.Manager
{
    public class DebugManager : RSLib.Framework.Singleton<DebugManager>, IConsoleProLoggable
    {
        [UnityEngine.SerializeField] private bool _dbgViewOn = false;

        public static bool DbgViewOn => Instance._dbgViewOn;

        public string ConsoleProPrefix => "Debug Manager";

        protected override void Awake()
        {
            base.Awake();

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("toggleDebugView", "Toggles debug view on screen.", true, false, DBG_ToggleDebugView));
        }

        [UnityEngine.ContextMenu("Toggle Debug View")]
        private void DBG_ToggleDebugView()
        {
            _dbgViewOn = !_dbgViewOn;
            ConsoleProLogger.Log(this, $"Debug View {(_dbgViewOn ? "on" : "off")}.", gameObject);
        }
    }
}