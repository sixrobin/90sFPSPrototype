namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    public class FPSCameraShake : FPSCameraExtraMovement, IConsoleProLoggable
    {
        [SerializeField] private Shake.ShakeSettings _settings = Shake.ShakeSettings.Default;
        [SerializeField] private bool _logsMuted = false;

        private Shake _shake = null;

        public string ConsoleProPrefix => "FPS Camera";

        public bool ConsoleProMuted => _logsMuted;

        public override void ApplyMovement()
        {
            ApplyShake();
        }

        public void AddTrauma(float value)
        {
            _shake.AddTrauma(value);
        }

        public void SetTrauma(float value)
        {
            _shake.SetTrauma(value);
        }

        public void ResetTrauma()
        {
            _shake.SetTrauma(0f);
        }

        public void SetShakePercentage(float value)
        {
            ConsoleProLogger.Log(this, $"Setting shake percentage to {value}%.", gameObject);
            _shake.SetMultiplier(value * 0.01f);
        }

        private void ApplyShake()
        {
            System.Tuple<Vector3, Quaternion> evaluatedShake = _shake.Evaluate(transform);
            if (evaluatedShake == null)
                return;

            transform.position += evaluatedShake.Item1;
            transform.rotation *= evaluatedShake.Item2;
        }

        private void Awake()
        {
            _shake = new Shake(_settings);
        }
    }
}