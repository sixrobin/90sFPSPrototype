namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    public class FPSHeadBob : FPSCameraExtraMovement
    {
        [Header("REFERENCES")]
        [SerializeField] private FPSController _fpsController = null;

        [Header("BOBBING SETTINGS")]
        [SerializeField] private float _idleAmplitude = 0.3f;
        [SerializeField] private float _idleSpeed = 0.35f;
        [SerializeField] private float _walkingAmplitude = 0.3f;
        [SerializeField] private float _walkingSpeed = 0.3f;
        [SerializeField] private float _crouchedAmplitude = 0.25f;
        [SerializeField] private float _crouchedSpeed = 0.2f;
        [SerializeField] private float _sprintingAmplitude = 0.55f;
        [SerializeField] private float _sprintingSpeed = 0.7f;
        [SerializeField, Min(0f)] private float _bobDamping = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _crouchWalkPercentage = 0.5f;

        private float _sineTimer;
        private float _currAmplitude;
        private float _currSpeed;
        private float _refAmplitude;
        private float _refSpeed;

        private float _offset;
        private bool _goingDown;

        public delegate void BobDirectionChangedEventHandler(int nextDir);
        public event BobDirectionChangedEventHandler BobDirectionChanged;

        public float Multiplier { get; private set; } = 1f;

        public override void ApplyMovement()
        {
            Bob();
        }

        public void SetPercentage(float mult)
        {
            ConsoleProLogger.Log(Manager.ReferencesHub.FPSMaster.FPSCamera, $"Setting head bob percentage to {mult}%.");
            Multiplier = mult * 0.01f;
        }

        /// <summary>
        /// Only calculates bobbing values without applying them.
        /// </summary>
        private void EvaluateBobbingValues()
        {
            float targetAmplitude;
            float targetSpeed;
            bool moving = _fpsController.CheckMovement();

            if (_fpsController.Crouched)
            {
                targetAmplitude = moving ? Mathf.Lerp(_crouchedAmplitude, _walkingAmplitude, _crouchWalkPercentage) : _crouchedAmplitude;
                targetSpeed = moving ? Mathf.Lerp(_crouchedSpeed, _walkingSpeed, _crouchWalkPercentage) : _crouchedSpeed;
            }
            else if (_fpsController.Sprinting)
            {
                targetAmplitude = _sprintingAmplitude;
                targetSpeed = _sprintingSpeed;
            }
            else
            {
                targetAmplitude = moving ? _walkingAmplitude : _idleAmplitude;
                targetSpeed = moving ? _walkingSpeed : _idleSpeed;
            }

            _currAmplitude = Mathf.SmoothDamp(_currAmplitude, targetAmplitude, ref _refAmplitude, _bobDamping);
            _currSpeed = Mathf.SmoothDamp(_currSpeed, targetSpeed, ref _refSpeed, _bobDamping);
        }

        private void Bob()
        {
            EvaluateBobbingValues();

            _sineTimer += Time.deltaTime * _currSpeed;
            float nextOffset = Mathf.Sin(_sineTimer) * _currAmplitude;

            if (_goingDown != nextOffset < _offset)
                BobDirectionChanged?.Invoke((int)Mathf.Sign(nextOffset - _offset));

            _goingDown = nextOffset < _offset;
            _offset = nextOffset;

            transform.position += new Vector3(0f, nextOffset * Multiplier);
        }

        private void ResetBobValues()
        {
            _sineTimer = 0f;
            _currAmplitude = 0f;
            _currSpeed = 0f;
        }
    }
}