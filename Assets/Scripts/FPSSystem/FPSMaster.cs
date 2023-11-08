﻿namespace Doomlike.FPSSystem
{
    using UnityEngine;

    /// <summary>
    /// Main class for the FPS controller. Controls the more generic things of the controller.
    /// </summary>
    public class FPSMaster : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private FPSComponent[] _allComponents = null;

        [Space(15)]
        [SerializeField] private FPSController _fpsController = null;
        [SerializeField] private FPSCamera _fpsCamera = null;
        [SerializeField] private FPSCameraShake _fpsCameraShake = null;
        [SerializeField] private FPSCameraAnimator _fpsCameraAnimator = null;
        [SerializeField] private FPSHealthSystem _fpsHealthSystem = null;
        [SerializeField] private FPSHeadBob _fpsHeadBob = null;
        [SerializeField] private FPSShoot _fpsShoot = null;
        [SerializeField] private FPSInteracter _fpsInteracter = null;
        [SerializeField] private FPSUIController _fpsUIController = null;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;
        [SerializeField] private bool _debugGodMode = false;

        private System.Collections.Generic.List<FPSControllableComponent> _allControllableComponents;

        public FPSController FPSController => _fpsController;

        public FPSCamera FPSCamera => _fpsCamera;

        public FPSCameraShake FPSCameraShake => _fpsCameraShake;

        public FPSCameraAnimator FPSCameraAnimator => _fpsCameraAnimator;

        public FPSHealthSystem FPSHealthSystem => _fpsHealthSystem;

        public FPSHeadBob FPSHeadBob => _fpsHeadBob;

        public FPSShoot FPSShoot => _fpsShoot;

        public FPSInteracter FPSInteracter => _fpsInteracter;

        public FPSUIController FPSUIController => _fpsUIController;

        public FPSInventory FPSInventory { get; private set; }

        public string ConsoleProPrefix => "FPS Master";

        public bool ConsoleProMuted => _logsMuted;

        public bool DebugGodMode => this._debugGodMode;

        /// <summary>
        /// Enables all the controllable components of the FPS controller.
        /// </summary>
        [ContextMenu("Enable All Components")]
        public void EnableAllComponents()
        {
            ConsoleProLogger.Log(this, $"Enabling all {_allControllableComponents.Count} components.", gameObject);
            for (int i = _allControllableComponents.Count - 1; i >= 0; --i)
                _allControllableComponents[i].SetControllability(true);
        }

        /// <summary>
        /// Disables all the controllable components of the FPS controller.
        /// </summary>
        [ContextMenu("Disable All Components")]
        public void DisableAllComponents()
        {
            ConsoleProLogger.Log(this, $"Disabling all {_allControllableComponents.Count} components.", gameObject);
            for (int i = _allControllableComponents.Count - 1; i >= 0; --i)
                _allControllableComponents[i].SetControllability(false);
        }

        public void DisableAllComponentsAtEndOfFrame()
        {
            StartCoroutine(DisableAllComponentsAtEndOfFrameCoroutine());
        }

        private void OnTerminalScreenToggled(bool state)
        {
            if (state)
            {
                DisableAllComponents();
                FPSUIController.Hide();
            }
            else
            {
                StartCoroutine(EnableAllComponentsAtEndOfFrameCoroutine());
                FPSUIController.Show();
            }
        }

        private System.Collections.IEnumerator EnableAllComponentsAtEndOfFrameCoroutine()
        {
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            EnableAllComponents();
        }

        private System.Collections.IEnumerator DisableAllComponentsAtEndOfFrameCoroutine()
        {
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            DisableAllComponents();
        }

        private void OnDialogueTriggered(DialogueSystem.ISentencesContainer dialogue)
        {
            DisableAllComponents();
            FPSUIController.Hide();
        }

        private void OnDialogueOver(DialogueSystem.ISentencesContainer dialogue)
        {
            StartCoroutine(EnableAllComponentsAtEndOfFrameCoroutine());
            FPSUIController.Show();
        }

        private void Awake()
        {
            _allControllableComponents = new System.Collections.Generic.List<FPSControllableComponent>();
            FPSInventory = new FPSInventory();

            for (int i = _allComponents.Length - 1; i >= 0; --i)
            {
                _allComponents[i].SetFPSMaster(this);
                if (_allComponents[i] is FPSControllableComponent controllableComponent)
                    _allControllableComponents.Add(controllableComponent);
            }

            if (Manager.ReferencesHub.TryGetTrainingWorkshopTerminalScreen(out UI.TrainingWorkshopTerminalScreen workshopTerminal))
                workshopTerminal.TerminalScreenToggled += OnTerminalScreenToggled;

            if (Manager.ReferencesHub.TryGetDialogueController(out DialogueSystem.DialogueController dialogueCtrl))
            {
                dialogueCtrl.DialogueTriggered += OnDialogueTriggered;
                dialogueCtrl.DialogueOver += OnDialogueOver;
            }

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("tgm", "Toggle God mode.", true, false, DBG_ToggleGodMode));
        }

        private void OnDestroy()
        {
            if (Manager.ReferencesHub.TryGetTrainingWorkshopTerminalScreen(out UI.TrainingWorkshopTerminalScreen workshopTerminal))
                workshopTerminal.TerminalScreenToggled -= OnTerminalScreenToggled;
        }

        [ContextMenu("Toggle God Mode")]
        private void DBG_ToggleGodMode()
        {
            this._debugGodMode = !this._debugGodMode;
            ConsoleProLogger.Log(this, $"God Mode {(this.DebugGodMode ? "on" : "off")}.", gameObject);
            Console.DebugConsole.LogExternal($"God Mode {(this.DebugGodMode ? "on" : "off")}.");
        }
    }
}