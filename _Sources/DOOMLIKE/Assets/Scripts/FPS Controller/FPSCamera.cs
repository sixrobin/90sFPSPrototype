namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    /// <summary>
    /// Main component of the FPS camera. Controls the camera position and rotation.
    /// Can be enabled/disabled if needed.
    /// </summary>
    public class FPSCamera : FPSControllableComponent, IConsoleProLoggable
    {
        [Header("REFERENCES")]
        [SerializeField] private FPSController _fpsController = null;
        [SerializeField] private FPSCameraExtraMovement[] _extraMovements = null;
        [SerializeField] private GameObject _scopeVisual = null;

        [Header("CAMERA SETTINGS")]
        [SerializeField] private float _pitchSpeed = 120;
        [SerializeField] private float _yawSpeed = 145;
        [SerializeField] private float _height = 1.8f;
        [SerializeField] private float _crouchedHeight = 0.7f;
        [SerializeField] private float _addForward = 0.15f;
        [SerializeField] private bool _xAxisReversed = false;
        [SerializeField] private bool _yAxisReversed = false;
        [SerializeField, Range(0f, 90f)] private float _minPitch = 60;
        [SerializeField, Range(0f, 90f)] private float _maxPitch = 60;

        private bool _pitchClamped = true;
        private float _initMinPitch;
        private float _initMaxPitch;

        private Vector3 _rawCamInput;
        private Vector3 _camDest;
        private Vector3 _currCamPos;
        private Vector3 _currCamEulerAngles;

        private float _xAxisSensitivityMult = 1f;
        private float _yAxisSensitivityMult = 1f;

        public string ConsoleProPrefix => "FPS Camera";

        public enum Axis
        {
            None,
            X,
            Y
        }

        public void ReverseAxis(Axis axis)
        {
            if (axis == Axis.None)
            {
                ConsoleProLogger.LogError(this, "Invalid axis for reverse.", gameObject);
                return;
            }

            if (axis == Axis.X)
            {
                ConsoleProLogger.Log(this, "Reversing X axis.", gameObject);
                _xAxisReversed = !_xAxisReversed;
            }
            else if (axis == Axis.Y)
            {
                ConsoleProLogger.Log(this, "Reversing Y axis.", gameObject);
                _yAxisReversed = !_yAxisReversed;
            }
        }

        public void SetAxisSensitivityMultiplier(Axis axis, float value)
        {
            if (axis == Axis.None)
            {
                ConsoleProLogger.LogError(this, "Invalid axis for sensitivity multiplier.", gameObject);
                return;
            }

            if (axis == Axis.X)
            {
                ConsoleProLogger.Log(this, $"Setting X axis sensitivity multiplier to {value}%.", gameObject);
                _yAxisSensitivityMult = value * 0.01f;
            }
            else if (axis == Axis.Y)
            {
                ConsoleProLogger.Log(this, $"Setting Y axis sensitivity multiplier to {value}%.", gameObject);
                _xAxisSensitivityMult = value * 0.01f;
            }
        }

        public void TogglePitchClamp(bool state)
        {
            _pitchClamped = state;
            _minPitch = _pitchClamped ? _initMinPitch : 90;
            _maxPitch = _pitchClamped ? _initMaxPitch : 90;
        }

        protected override void OnControlAllowed()
        {
            base.OnControlAllowed();
            _scopeVisual.SetActive(true);
        }

        protected override void OnControlDisallowed()
        {
            base.OnControlDisallowed();
            _scopeVisual.SetActive(false);
            _rawCamInput.x = 0;
            _rawCamInput.y = 0;
        }

        private void GetInputs()
        {
            _rawCamInput.x = Input.GetAxisRaw("Mouse Y");
            _rawCamInput.y = Input.GetAxisRaw("Mouse X");
        }

        private void EvaluateDestination()
        {
            _camDest = _fpsController.transform.position;
            _camDest += _fpsController.transform.up * (_fpsController.Crouched ? _crouchedHeight : _height);
            _camDest += transform.forward * _addForward;
            _currCamPos.x = _camDest.x;
            _currCamPos.z = _camDest.z;
            _currCamPos.y = Mathf.Lerp(_currCamPos.y, _camDest.y, Time.deltaTime * 10);

            _rawCamInput.x *= _pitchSpeed;
            _rawCamInput.y *= _yawSpeed;
            _rawCamInput *= Time.deltaTime;

            _currCamEulerAngles.y += _rawCamInput.y * (_xAxisReversed ? -1 : 1) * _yAxisSensitivityMult;
            _currCamEulerAngles.x += _rawCamInput.x * (_yAxisReversed ? 1 : -1) * _xAxisSensitivityMult;
            _currCamEulerAngles.x = Mathf.Clamp(_currCamEulerAngles.x, -_minPitch, _maxPitch);

            while (_currCamEulerAngles.x > 360) _currCamEulerAngles.x -= 360;
            while (_currCamEulerAngles.y > 360) _currCamEulerAngles.y -= 360;
            while (_currCamEulerAngles.x < -360) _currCamEulerAngles.x += 360;
            while (_currCamEulerAngles.y < -360) _currCamEulerAngles.y += 360;
        }

        private void Position()
        {
            transform.position = _currCamPos;
            transform.localEulerAngles = _currCamEulerAngles;
        }

        private void ApplyExtraMovements()
        {
            foreach (FPSCameraExtraMovement extraMovement in _extraMovements)
                extraMovement.ApplyMovement();
        }

        private void OnOptionsStateChanged(bool state)
        {
            _scopeVisual.SetActive(!state);
        }

        private void Awake()
        {
            _currCamEulerAngles = transform.localEulerAngles;
            _initMinPitch = _minPitch;
            _initMaxPitch = _maxPitch;
        }

        private void Start()
        {
            FPSMaster.OptionsManager.OptionsStateChanged += OnOptionsStateChanged;
        }

        private void Update()
        {
            if (Controllable)
                GetInputs();
        }

        private void LateUpdate()
        {
            EvaluateDestination();
            Position();
            ApplyExtraMovements();
        }
    }
}