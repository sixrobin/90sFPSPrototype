namespace Doomlike.Manager
{
    using UnityEngine;

    public class ReferencesHub : RSLib.Framework.Singleton<ReferencesHub>
    {
        [SerializeField] private FPSSystem.FPSMaster _fpsMaster = null;
        [SerializeField] private OptionsManager _optionsManager = null;
        [SerializeField] private TimeManager _timeManager = null;
        [SerializeField] private CursorManager _cursorManager = null;
        [SerializeField] private UI.TrainingWorkshopTerminalScreen _trainingWorkshopTerminalScreen = null;
        [SerializeField] private DialogueSystem.DialogueController _dialogueController = null;

        [Header("MISC")]
        [SerializeField] private GameObject _prototypeEndCanvas = null;

        public static FPSSystem.FPSMaster FPSMaster => Instance._fpsMaster;

        public static OptionsManager OptionsManager => Instance._optionsManager;

        public static TimeManager TimeManager => Instance._timeManager;

        public static CursorManager CursorManager => Instance._cursorManager;

        public static UI.TrainingWorkshopTerminalScreen TrainingWorkshopTerminalScreen => Instance._trainingWorkshopTerminalScreen;

        public static DialogueSystem.DialogueController DialogueController => Instance._dialogueController;

        public static GameObject PrototypeEndCanvas => Instance._prototypeEndCanvas;

        public static bool TryGetOptionsManager(out OptionsManager optionsManager)
        {
            optionsManager = Exists() ? OptionsManager : null;
            return optionsManager != null;
        }

        public static bool TryGetCursorManager(out CursorManager cursorManager)
        {
            cursorManager = Exists() ? CursorManager : null;
            return cursorManager != null;
        }

        public static bool TryGetTrainingWorkshopTerminalScreen(out UI.TrainingWorkshopTerminalScreen terminalScreen)
        {
            terminalScreen = Exists() ? TrainingWorkshopTerminalScreen : null;
            return terminalScreen != null;
        }

        public static bool TryGetDialogueController(out DialogueSystem.DialogueController dialogueCtrl)
        {
            dialogueCtrl = Exists() ? DialogueController : null;
            return dialogueCtrl != null;
        }

        [ContextMenu("Locate References")]
        private void LocateReferences()
        {
            Instance._fpsMaster = FindObjectOfType<FPSSystem.FPSMaster>();
            Instance._optionsManager = FindObjectOfType<OptionsManager>();
            Instance._timeManager = FindObjectOfType<TimeManager>();
            Instance._cursorManager = FindObjectOfType<CursorManager>();
            Instance._trainingWorkshopTerminalScreen = FindObjectOfType<UI.TrainingWorkshopTerminalScreen>();
            Instance._dialogueController = FindObjectOfType<DialogueSystem.DialogueController>();
        }
    }
}