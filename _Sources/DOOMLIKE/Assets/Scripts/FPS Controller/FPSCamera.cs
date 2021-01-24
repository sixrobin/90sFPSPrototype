namespace Doomlike.FPSCtrl
{
    using RSLib.Extensions;
    using UnityEngine;

    /// <summary>
    /// Main component of the FPS camera. Controls the camera position and rotation.
    /// Can be enabled/disabled if needed.
    /// </summary>
    public class FPSCamera : FPSControllableComponent, IConsoleProLoggable
    {
        private const string INPUT_MOUSEX = "Mouse X";
        private const string INPUT_MOUSEY = "Mouse Y";

        [Header("REFERENCES")]
        [SerializeField] private FPSController _fpsController = null;
        [SerializeField] private FPSCameraExtraMovement[] _extraMovements = null;
        [SerializeField] private CameraRampsController _camRampsController = null;
        [SerializeField] private GameObject _scopeVisual = null;

        [Header("CAMERA SETTINGS")]
        [SerializeField] private float _pitchSpeed = 120f;
        [SerializeField] private float _yawSpeed = 145f;
        [SerializeField] private float _height = 1.8f;
        [SerializeField] private float _crouchedHeight = 0.7f;
        [SerializeField] private float _addForward = 0.15f;
        [SerializeField] private bool _xAxisReversed = false;
        [SerializeField] private bool _yAxisReversed = false;
        [SerializeField, Range(0f, 90f)] private float _minPitch = 60f;
        [SerializeField, Range(0f, 90f)] private float _maxPitch = 60f;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;

        private bool _pitchClamped = true;
        private float _initMinPitch;
        private float _initMaxPitch;
        private float _initHeight;

        private Vector3 _rawCamInput;
        private Vector3 _camDest;
        private Vector3 _currCamPos;
        private Vector3 _currCamEulerAngles;

        private bool _recentering;

        private float _xAxisSensiMult = 1f;
        private float _yAxisSensiMult = 1f;

        public enum Axis
        {
            None,
            X,
            Y
        }

        public string ConsoleProPrefix => "FPS Camera";

        public bool ConsoleProMuted => _logsMuted;

        public CameraRampsController CamRampsController => _camRampsController;

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
                _yAxisSensiMult = value * 0.01f;
            }
            else if (axis == Axis.Y)
            {
                ConsoleProLogger.Log(this, $"Setting Y axis sensitivity multiplier to {value}%.", gameObject);
                _xAxisSensiMult = value * 0.01f;
            }
        }

        public void TogglePitchClamp(bool state)
        {
            _pitchClamped = state;
            _minPitch = _pitchClamped ? _initMinPitch : 90f;
            _maxPitch = _pitchClamped ? _initMaxPitch : 90f;
        }

        public void Recenter(float dur)
        {
            StartCoroutine(RecenterCoroutine(dur));
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
            _rawCamInput.x = 0f;
            _rawCamInput.y = 0f;
        }

        private void GetInputs()
        {
            _rawCamInput.x = Input.GetAxisRaw(INPUT_MOUSEY);
            _rawCamInput.y = Input.GetAxisRaw(INPUT_MOUSEX);
        }

        private void EvaluateDestination()
        {
            _camDest = _fpsController.transform.position;
            _camDest += _fpsController.transform.up * (_fpsController.Crouched ? _crouchedHeight : _height);
            _camDest += transform.forward * _addForward;

            _currCamPos.x = _camDest.x;
            _currCamPos.z = _camDest.z;
            _currCamPos.y = Mathf.Lerp(_currCamPos.y, _camDest.y, Time.deltaTime * 10f);

            _rawCamInput.x *= _pitchSpeed;
            _rawCamInput.y *= _yawSpeed;
            _rawCamInput *= Time.deltaTime;

            if (!_recentering)
            {
                _currCamEulerAngles.y += _rawCamInput.y * (_xAxisReversed ? -1 : 1) * _yAxisSensiMult;
                _currCamEulerAngles.x += _rawCamInput.x * (_yAxisReversed ? 1 : -1) * _xAxisSensiMult;
                _currCamEulerAngles.x = Mathf.Clamp(_currCamEulerAngles.x, -_minPitch, _maxPitch);

                while (_currCamEulerAngles.x > 360f) _currCamEulerAngles.x -= 360f;
                while (_currCamEulerAngles.y > 360f) _currCamEulerAngles.y -= 360f;
                while (_currCamEulerAngles.x < -360f) _currCamEulerAngles.x += 360f;
                while (_currCamEulerAngles.y < -360f) _currCamEulerAngles.y += 360f;
            }
        }

        private void Position()
        {
            transform.position = _currCamPos;
            transform.localEulerAngles = _currCamEulerAngles;
        }

        private void ApplyExtraMovements()
        {
            for (int i = _extraMovements.Length - 1; i >= 0; --i)
                _extraMovements[i].ApplyMovement();
        }

        private void OnOptionsStateChanged(bool state)
        {
            _scopeVisual.SetActive(!state);
        }

        private System.Collections.IEnumerator RecenterCoroutine(float degreesPerSecond)
        {
            _recentering = true;

            float initXSign = Mathf.Sign(_currCamEulerAngles.x);
            while (initXSign > 0f ? _currCamEulerAngles.x > 0f : _currCamEulerAngles.x < 0f)
            {
                _currCamEulerAngles = _currCamEulerAngles.AddX((initXSign > 0f ? -degreesPerSecond : degreesPerSecond) * Time.deltaTime);
                yield return null;
            }

            _currCamEulerAngles = _currCamEulerAngles.WithX(0f);
            _recentering = false;
        }

        private void Awake()
        {
            if (Manager.ReferencesHub.TryGetOptionsManager(out Manager.OptionsManager optionsManager))
                optionsManager.OptionsStateChanged += OnOptionsStateChanged;

            _currCamEulerAngles = transform.localEulerAngles;
            _initMinPitch = _minPitch;
            _initMaxPitch = _maxPitch;
            _initHeight = _height;

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand<float>("camHeight", "Sets the camera height.", true, false, DBG_SetHeight));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("resetCamHeight", "Resets the camera height.", true, false, DBG_ResetHeight));
        }

        private void Update()
        {
            if (!Controllable)
                return;

            GetInputs();
        }

        private void LateUpdate()
        {
            EvaluateDestination();
            Position();
            ApplyExtraMovements();
        }

        private void OnDestroy()
        {
            if (Manager.ReferencesHub.TryGetOptionsManager(out Manager.OptionsManager optionsManager))
                optionsManager.OptionsStateChanged -= OnOptionsStateChanged;
        }

        private void DBG_SetHeight(float h)
        {
            _height = h;
        }

        private void DBG_ResetHeight()
        {
            _height = _initHeight;
        }
    }
}