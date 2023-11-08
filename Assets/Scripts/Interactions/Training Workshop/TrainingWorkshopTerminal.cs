﻿namespace Doomlike
{
    using UnityEngine;

    public class TrainingWorkshopTerminal : OutlinedInteraction, FPSSystem.IFPSShootable, IConsoleProLoggable
    {
        [SerializeField] private TrainingWorkshop _trainingWorkshop = null;
        [SerializeField] private MeshRenderer _terminalRenderer = null;
        [SerializeField] private Material _terminalOnMaterial = null;
        [SerializeField] private GameObject _screenShatter = null;
        [SerializeField] private ParticleSystem _screenShatterParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.13f;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;

        private Material _terminalOffMaterial;
        private bool _isOn;

        public bool ScreenShattered { get; private set; }

        public TrainingWorkshop TrainingWorkshop => _trainingWorkshop;

        public float TraumaOnShot => _traumaOnShot;

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public string ConsoleProPrefix => "Training Workshop";

        public bool ConsoleProMuted => _logsMuted;

        public override void Interact()
        {
            base.Interact();

            if (_isOn)
                return;

            _isOn = true;
            if (!ScreenShattered)
                _terminalRenderer.material = _terminalOnMaterial;

            _trainingWorkshop.ResetWorkshopInstantly();

            ConsoleProLogger.Log(this, $"Opening Training Workshop terminal <b>{transform.name}</b> for Workshop {TrainingWorkshop.WorkshopIndex} :\n" +
                $"Tries: {_trainingWorkshop.Tries},\n" +
                $"Best time: {(_trainingWorkshop.BestTime == float.MaxValue ? 0f : _trainingWorkshop.BestTime)} seconds,\n" +
                $"Best shots: {(_trainingWorkshop.BestShots == int.MaxValue ? 0 : _trainingWorkshop.BestShots)} shots,\n" +
                $"Score: {_trainingWorkshop.BestScore}.\n" +
                $"(If terminal won't open, make sure the terminal screen is listening to this interaction event.)", gameObject);
        }

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            if (ScreenShattered)
                return;

            ConsoleProLogger.Log(this, $"Shattering Training Workshop terminal <b>{transform.name}</b>'s screen.", gameObject);
            ScreenShattered = true;
            _screenShatter.SetActive(true);
            _screenShatterParticles.Play();
        }

        public void ShutdownTerminal()
        {
            ConsoleProLogger.Log(this, $"Shutting down Training Workshop terminal <b>{transform.name}</b>.", gameObject);
            _isOn = false;
            _terminalRenderer.material = _terminalOffMaterial;
        }

        private void OnTerminalScreenToggled(bool state)
        {
            if (!state)
                ShutdownTerminal();
        }

        protected override void Awake()
        {
            base.Awake();

            if (Manager.ReferencesHub.TryGetTrainingWorkshopTerminalScreen(out UI.TrainingWorkshopTerminalScreen workshopTerminal))
                workshopTerminal.TerminalScreenToggled += OnTerminalScreenToggled;

            _terminalOffMaterial = _terminalRenderer.material;
        }

        private void OnDestroy()
        {
            if (Manager.ReferencesHub.TryGetTrainingWorkshopTerminalScreen(out UI.TrainingWorkshopTerminalScreen workshopTerminal))
                workshopTerminal.TerminalScreenToggled -= OnTerminalScreenToggled;
        }
    }
}