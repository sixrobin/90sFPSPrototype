﻿namespace Doomlike.FPSSystem
{
    using RSLib.Extensions;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class FPSController : FPSControllableComponent, IConsoleProLoggable
    {
        private const string INPUT_HOR = "Horizontal";
        private const string INPUT_VER = "Vertical";
        private const string INPUT_CROUCH = "Crouch";
        private const string INPUT_SPRINT = "Sprint";

        [Header("CONTROLLER OPTIONS")]
        [SerializeField] private bool _canSprint = true;
        [SerializeField] private bool _canCrouch = true;

        [Header("REFERENCES")]
        [SerializeField] private Transform _camTransform = null;

        [Header("MOVEMENT")]
        [SerializeField] private float _baseSpeed = 3.2f;
        [SerializeField] private float _crouchedSpeed = 1.5f;
        [SerializeField] private float _sprintSpeed = 7.8f;
        [SerializeField] private float _speedDampingTime = 0.2f;
        [SerializeField] private float _capsuleCrouchedHeight = 0.8f;
        [SerializeField] private float _inputDamping = 0.1f;
        [SerializeField] private float _uncrouchCheckAccuracy = 0.3f;

        [Header("STAMINA")]
        [SerializeField] private float _fullSprintDur = 5;
        [SerializeField] private float _recoverDur = 2;
        [SerializeField] private float _fullReloadDur = 15;

        [Header("MISC")]
        [SerializeField] private LayerMask _groundMask = 0;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;
        [SerializeField] private bool _debug = true;

        private Rigidbody _rb;
        private CapsuleCollider _capsule;

        private float _baseCapsuleHeight;
        private float _baseCapsuleYCenter;
        private Vector3 _rawMovementInput;
        private Vector3 _currMovementInput;
        private Vector3 _refMovementInput;
        private Vector3 _currentVelocity;
        private float _currentSpeed;
        private float _refSpeed;

        //private bool _falling;

        private float _debugMoveSpeed = -1f;
        private bool _debugNoCollisionsMode;

        public FPSStaminaManager StaminaManager { get; set; }

        private bool _sprinting;
        public bool Sprinting
        {
            get => _sprinting;
            private set
            {
                if (value)
                    _sprinting = CheckMovement();
                else
                    _sprinting = value;
            }
        }

        private bool _crouched;
        public bool Crouched
        {
            get => _crouched;
            private set
            {
                if (value && !_crouched)
                {
                    _capsule.center = _capsule.center.WithY(_baseCapsuleYCenter / (_baseCapsuleHeight / _capsuleCrouchedHeight));
                    _capsule.height = _capsuleCrouchedHeight;
                    _crouched = true;
                }
                else if (!value && _crouched)
                {
                    if (!CheckUncrouchAbility())
                        return;

                    _capsule.center = _capsule.center.WithY(_baseCapsuleYCenter);
                    _capsule.height = _baseCapsuleHeight;
                    _crouched = false;
                }
            }
        }

        public string ConsoleProPrefix => "FPS Controller";

        public bool ConsoleProMuted => _logsMuted;

        public bool CheckGround()
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, 0.1f, _groundMask);
        }

        public bool CheckMovement()
        {
            return _rawMovementInput.sqrMagnitude > 0f;
        }

        public bool CheckUncrouchAbility()
        {
            for (int i = 0; i < 4; ++i)
            {
                float a = 90f * i * Mathf.Deg2Rad;
                Vector3 raycastStart = transform.position + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * _uncrouchCheckAccuracy;
                if (Physics.Raycast(raycastStart, Vector3.up, _baseCapsuleHeight + 0.1f, _groundMask))
                    return false;
            }

            return true;
        }

        protected override void OnControlDisallowed()
        {
            base.OnControlDisallowed();
            _rawMovementInput = Vector3.zero;
            _currMovementInput = Vector3.zero;
            Sprinting = false;
        }

        /// <summary>
        /// Stores the controller inputs to use them later.
        /// </summary>
        private void GetInputs()
        {
            _rawMovementInput.x = Input.GetAxisRaw(INPUT_HOR);
            _rawMovementInput.z = Input.GetAxisRaw(INPUT_VER);
            _currMovementInput = Vector3.SmoothDamp(_currMovementInput, _rawMovementInput.normalized, ref _refMovementInput, _inputDamping);

            // TODO: Change behaviour if some option is set to Get button down for sprinting and crouching.
            Sprinting = _canSprint && !Crouched && !StaminaManager.IsEmpty && Input.GetButton(INPUT_SPRINT);
            Crouched = _canCrouch && Input.GetButton(INPUT_CROUCH);
        }

        /// <summary>
        /// Calculates the target speed to use depending on the controller state.
        /// </summary>
        private void EvaluateSpeed()
        {
            if (_debugMoveSpeed > -1f)
            {
                this._currentSpeed = _debugMoveSpeed;
                return;
            }

            float targetSpeed = Sprinting ? _sprintSpeed : (_crouched ? _crouchedSpeed : _baseSpeed);
            this._currentSpeed = Mathf.SmoothDamp(this._currentSpeed, targetSpeed, ref _refSpeed, _speedDampingTime);
        }

        /// <summary>
        /// Applies the calculated values to the controller's attached rigidbody.
        /// </summary>
        private void MoveBody()
        {
            EvaluateSpeed();

            if (_debugNoCollisionsMode)
            {
                MoveBodyWithoutCollisions();
                return;
            }

            Quaternion cameraForward = Quaternion.Euler(0f, _camTransform.localEulerAngles.y, 0f);
            this._currentVelocity = cameraForward * _currMovementInput;
            this._currentVelocity *= this._currentSpeed;
            this._currentVelocity.y = _rb.velocity.y;

            _rb.velocity = this._currentVelocity;

            if (_debug)
                Debug.DrawLine(transform.position, transform.position + _rb.velocity, Color.yellow);
        }

        private void MoveBodyWithoutCollisions()
        {
            // This is called in FixedUpdate, but it is a debug method so it's not a real issue.

            Quaternion camForward = Quaternion.Euler(_camTransform.localEulerAngles);
            this._currentVelocity = camForward * _currMovementInput;
            this._currentVelocity *= this._currentSpeed;

            transform.Translate(this._currentVelocity * Time.deltaTime);
        }

        private void UpdateRigidbodyConstraints()
        {
            RigidbodyConstraints constraints = RigidbodyConstraints.FreezeRotation;
            if (!CheckMovement() && CheckGround())
                constraints |= RigidbodyConstraints.FreezePositionY;

            _rb.constraints = constraints;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();

            _baseCapsuleHeight = _capsule.height;
            _baseCapsuleYCenter = _capsule.center.y;

            StaminaManager = new FPSStaminaManager(_fullSprintDur, _recoverDur, _fullReloadDur);

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("tcl", "Toggle collisions.", true, false, DBG_ToggleCollisions));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand<float>("setSpeed", "Sets the player movement speed.", true, false, DBG_SetMoveSpeed));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("resetSpeed", "Resets the player movement speed.", true, false, DBG_ResetMoveSpeed));
        }

        private void Update()
        {
            if (_debug && Crouched)
            {
                for (int i = 0; i < 4; ++i)
                {
                    float a = 90 * i * Mathf.Deg2Rad;
                    Vector3 raycastStart = transform.position + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * _uncrouchCheckAccuracy;
                    Debug.DrawLine(raycastStart, raycastStart + Vector3.up * (_baseCapsuleHeight + 0.1f), Color.cyan);
                }
            }

            if (_canSprint)
                StaminaManager.Update(Sprinting && !FPSMaster.DebugGodMode);

            if (Controllable)
                GetInputs();
        }

        private void FixedUpdate()
        {
            MoveBody();
            UpdateRigidbodyConstraints();
        }

        private void DBG_SetMoveSpeed(float speed)
        {
            if (speed < 0f)
            {
                Console.DebugConsole.LogExternalError("Can not set speed to a negative value.");
                return;
            }

            _debugMoveSpeed = speed;
        }

        private void DBG_ResetMoveSpeed()
        {
            _debugMoveSpeed = -1f;
            Console.DebugConsole.LogExternal($"Reset player speed to {_baseSpeed}.");
        }

        [ContextMenu("Toggle Collisions")]
        private void DBG_ToggleCollisions()
        {
            _debugNoCollisionsMode = !_debugNoCollisionsMode;
            this.Log($"Collisions {(_debugNoCollisionsMode ? "off" : "on")}.", gameObject);
            Console.DebugConsole.LogExternal($"Collisions {(_debugNoCollisionsMode ? "off" : "on")}.");

            _rb.isKinematic = _debugNoCollisionsMode;
            _capsule.enabled = !_debugNoCollisionsMode;
            FPSMaster.FPSCamera.TogglePitchClamp(!_debugNoCollisionsMode);
        }
    }
}