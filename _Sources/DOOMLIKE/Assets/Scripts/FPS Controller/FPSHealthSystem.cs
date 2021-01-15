﻿namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    public class FPSHealthSystem : FPSComponent, IConsoleProLoggable
    {
        [SerializeField] private int _initHealth = 100;

        [Header("DEATH GRAYSCALE FADE")]
        [SerializeField] private float _delay = 0.6f;
        [SerializeField] private float _desaturationDur = 2.8f;
        [SerializeField] private float _pauseDur = 0.15f;
        [SerializeField] private float _fadeToBlackDur = 2.3f;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;

        private Collider _hitbox;

        public RSLib.HealthSystem HealthSystem { get; private set; }

        public string ConsoleProPrefix => "FPS Health System";

        public bool ConsoleProMuted => _logsMuted;

        public void Damage(int dmg, float trauma)
        {
            if (FPSMaster.DbgGodMode)
                return;

            UnityEngine.Assertions.Assert.IsFalse(HealthSystem.IsDead, "Damaging an already dead health system owner.");

            HealthSystem.Damage(dmg);
            ConsoleProLogger.Log(this, $"Received <b>{dmg}</b> damages, <b>{HealthSystem.Health}</b> health left.", gameObject);

            FPSMaster.FPSCameraShake.AddTrauma(HealthSystem.IsDead ? 0.2f : trauma);
            if (!HealthSystem.IsDead)
                FPSMaster.FPSCameraAnimator.PlayHurtAnimation();
        }

        private void OnKilled()
        {
            // Player death.

            FPSMaster.DisableAllComponents();
            _hitbox.enabled = false;

            FPSMaster.FPSUIController.Hide();
            FPSMaster.FPSHeadBob.SetState(false);
            FPSMaster.FPSCamera.Recenter(180f);
            FPSMaster.FPSCameraAnimator.PlayDeathAnimation();
            FPSMaster.FPSCamera.CamRampsController.FadeToDeathGrayscale(_delay, _desaturationDur, _pauseDur, _fadeToBlackDur);
        }

        private void Awake()
        {
            // No RequireComponent because the collider type is variable.
            if (!TryGetComponent(out _hitbox))
                Debug.LogWarning("FPSHealthSystem WARNING: gameObject doesn't have a collider and can't be damaged.", gameObject);

            HealthSystem = new RSLib.HealthSystem(_initHealth);
            HealthSystem.Killed += OnKilled;

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("killPlayer", "Instantly kills the player.", true, false, OnKilled));
        }
    }
}