namespace Doomlike.Manager
{
    using UnityEngine;

    public class ReferencesHub : RSLib.Framework.Singleton<ReferencesHub>
    {
        [SerializeField] private FPSCtrl.FPSMaster _fpsMaster = null;
        [SerializeField] private OptionsManager _optionsManager = null;
        [SerializeField] private TimeManager _timeManager = null;
        [SerializeField] private CursorManager _cursorManager = null;
        [SerializeField] private UI.TrainingWorkshopTerminalScreen _trainingWorkshopTerminalScreen = null;

        [Header("MISC")]
        [SerializeField] private GameObject _prototypeEndCanvas = null;

        public static FPSCtrl.FPSMaster FPSMaster => Instance._fpsMaster;

        public static OptionsManager OptionsManager => Instance._optionsManager;

        public static TimeManager TimeManager => Instance._timeManager;

        public static CursorManager CursorManager => Instance._cursorManager;

        public static UI.TrainingWorkshopTerminalScreen TrainingWorkshopTerminalScreen => Instance._trainingWorkshopTerminalScreen;

        public static GameObject PrototypeEndCanvas => Instance._prototypeEndCanvas;
    }
}