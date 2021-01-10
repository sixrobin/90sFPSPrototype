﻿namespace Doomlike.FPSCtrl
{
    /// <summary>
    /// Stamina manager of the FPS controller. Does not inherit from MonoBehaviour and has an Update
    /// method that should be call in some MonoBehaviour Update.
    /// Must be constructed to be initialized with the wanted settings.
    /// </summary>
    public class FPSStaminaManager : IConsoleProLoggable
    {
        private float _fullDur;
        private float _recoverDelay;
        private float _reloadDur;
        private bool _recovering;
        private float _recoverTimer;

        public delegate void OutOfStaminaEventHandler();

        public event OutOfStaminaEventHandler OutOfStamina;

        private float _currentCharge;
        public float CurrentCharge
        {
            get => _currentCharge;
            private set => _currentCharge = UnityEngine.Mathf.Clamp01(value);
        }

        public bool IsEmpty => CurrentCharge == 0f;

        public string ConsoleProPrefix => "FPS Stamina Manager";

        public FPSStaminaManager()
        {
            SetSettings(5f, 2f, 15f);
            CurrentCharge = 1f;
        }

        public FPSStaminaManager(float fullDuration, float recoverDelay, float reloadDuration)
        {
            SetSettings(fullDuration, recoverDelay, reloadDuration);
            CurrentCharge = 1f;
        }

        /// <summary>
        /// Overrides the stamina settings at runtime.
        /// </summary>
        /// <param name="fullDur">Full stamina charge duration.</param>
        /// <param name="recoverDelay">Delay to wait when stamina is empty.</param>
        /// <param name="reloadDur">Full reload duration from empty to full.</param>
        public void SetSettings(float fullDur, float recoverDelay, float reloadDur)
        {
            ConsoleProLogger.Log(this, "Setting stamina stats :\n" +
                $"Full duration: {fullDur},\n" +
                $"Recover delay: {recoverDelay},\n" +
                $"Reload duration: {reloadDur}.\n");

            _fullDur = fullDur;
            _recoverDelay = recoverDelay;
            _reloadDur = reloadDur;
        }

        /// <summary>
        /// Updates the stamina charge according to the stamina current state (reloading, empty, etc.).
        /// Must be called inside a MonoBehaviour Update method.
        /// </summary>
        /// <param name="consuming">Is the stamina being consumed.</param>
        public void Update(bool consuming)
        {
            if (_recovering)
            {
                _recoverTimer += UnityEngine.Time.deltaTime;
                if (_recoverTimer > _recoverDelay)
                {
                    _recoverTimer = 0;
                    _recovering = false;
                }

                return;
            }

            if (consuming)
            {
                CurrentCharge -= UnityEngine.Time.deltaTime / _fullDur;
                if (CurrentCharge == 0)
                {
                    ConsoleProLogger.Log(this, "Out of stamina.");
                    OutOfStamina?.Invoke();
                    _recovering = true;
                }
            }
            else
            {
                CurrentCharge += UnityEngine.Time.deltaTime / _reloadDur;
            }
        }
    }
}