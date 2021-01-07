using RSLib.Extensions;
using UnityEngine;

[RequireComponent (typeof (Rigidbody), typeof (CapsuleCollider))]
public class FPSController : FPSControllableComponent
{
    [Header("CONTROLLER OPTIONS")]
    [SerializeField] private bool _canSprint = true;
    [SerializeField] private bool _canCrouch = true;

    [Header("REFERENCES")]
    [SerializeField] private Transform _cameraTransform = null;

    [Header("MOVEMENT")]
    [SerializeField] private float _baseSpeed = 3.2f;
    [SerializeField] private float _crouchedSpeed = 1.5f;
    [SerializeField] private float _sprintSpeed = 7.8f;
    [SerializeField] private float _speedDampingTime = 0.2f;
    [SerializeField] private float _capsuleCrouchedHeight = 0.8f;
    [SerializeField] private float _inputDamping = 0.1f;
    [SerializeField] private float _uncrouchCheckAccuracy = 0.3f;

    [Header("STAMINA")]
    [SerializeField] private float _fullSprintDuration = 5;
    [SerializeField] private float _recoverDuration = 2;
    [SerializeField] private float _fullReloadDuration = 15;

    [Header("MISC")]
    [SerializeField] private LayerMask _groundMask = 0;

    [Header("DEBUG")]
    [SerializeField] private bool _dbg = true;

    public FPSStaminaManager StaminaManager { get; set; }

    private Transform m_Transform;
    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_Capsule;

    private float _baseCapsuleHeight;
    private float _baseCapsuleYCenter;
    private Vector3 _rawMovementInput;
    private Vector3 _currentMovementInput;
    private Vector3 _refMovementInput;
    private Vector3 _currentVelocity;
    private float _currentSpeed;
    private float _refSpeed;

    private bool _sprinting;
    public bool Sprinting
    {
        get => _sprinting;
        private set
        {
            if (value)
                _sprinting = IsMoving;
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
                m_Capsule.center = m_Capsule.center.WithY(_baseCapsuleYCenter / (_baseCapsuleHeight / _capsuleCrouchedHeight));
                m_Capsule.height = _capsuleCrouchedHeight;
                _crouched = value;
            }
            else if (!value && _crouched)
            {
                if (!CheckUncrouchAbility())
                    return;

                m_Capsule.center = m_Capsule.center.WithY(_baseCapsuleYCenter);
                m_Capsule.height = _baseCapsuleHeight;
                _crouched = value;
            }
        }
    }

    public bool IsMoving => _rawMovementInput.sqrMagnitude > 0;

    protected override void OnControlDisallowed()
    {
        base.OnControlDisallowed();
        _rawMovementInput = Vector3.zero;
        _currentMovementInput = Vector3.zero;
        Sprinting = false;
    }

    public bool CheckGround()
    {
        return Physics.Raycast(m_Transform.position + Vector3.up * 0.05f, Vector3.down, 0.1f, _groundMask);
    }

    public bool CheckUncrouchAbility()
    {
        for (int i = 0; i < 4; ++i)
        {
            float a = 90 * i * Mathf.Deg2Rad;
            Vector3 raycastStart = m_Transform.position + new Vector3 (Mathf.Cos(a), 0, Mathf.Sin(a)) * _uncrouchCheckAccuracy;
            if (Physics.Raycast (raycastStart, Vector3.up, _baseCapsuleHeight + 0.1f, _groundMask))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Stores the controller inputs to use them later.
    /// </summary>
    void GetInputs()
    {
        _rawMovementInput.x = Input.GetAxisRaw ("Horizontal");
        _rawMovementInput.z = Input.GetAxisRaw ("Vertical");
        _currentMovementInput = Vector3.SmoothDamp (_currentMovementInput, _rawMovementInput.normalized, ref _refMovementInput, _inputDamping);

        Sprinting = _canSprint && !Crouched && !StaminaManager.IsEmpty && Input.GetButton("Sprint");
        Crouched = _canCrouch && Input.GetButton("Crouch");
    }

    /// <summary>
    /// Calculates the target speed to use depending on the controller state.
    /// </summary>
    void EvaluateSpeed()
    {
        float targetSpeed = Sprinting ? _sprintSpeed : (_crouched ? _crouchedSpeed : _baseSpeed);
        _currentSpeed = Mathf.SmoothDamp (_currentSpeed, targetSpeed, ref _refSpeed, _speedDampingTime);
    }

    /// <summary>
    /// Applies the calculated values to the controller's attached rigidbody.
    /// </summary>
    void MoveBody()
    {
        EvaluateSpeed();

        Quaternion cameraForward = Quaternion.Euler (0, _cameraTransform.localEulerAngles.y, 0);
        _currentVelocity = cameraForward * _currentMovementInput;
        _currentVelocity *= _currentSpeed;
        _currentVelocity.y = m_Rigidbody.velocity.y;

        m_Rigidbody.velocity = _currentVelocity;

        if (_dbg)
            Debug.DrawLine(m_Transform.position, m_Transform.position + m_Rigidbody.velocity, Color.yellow);
    }

    void Awake()
    {
        m_Transform = transform;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();

        StaminaManager = new FPSStaminaManager(_fullSprintDuration, _recoverDuration, _fullReloadDuration);
        _baseCapsuleHeight = m_Capsule.height;
        _baseCapsuleYCenter = m_Capsule.center.y;
    }

    void Update()
    {
        if (Crouched && _dbg)
        {
            for (int i = 0; i < 4; ++i)
            {
                float a = 90 * i * Mathf.Deg2Rad;
                Vector3 raycastStart = m_Transform.position + new Vector3 (Mathf.Cos(a), 0, Mathf.Sin(a)) * _uncrouchCheckAccuracy;
                Debug.DrawLine(raycastStart, raycastStart + Vector3.up * (_baseCapsuleHeight + 0.1f), Color.cyan);
            }
        }

        if (_canSprint)
            StaminaManager.Update(Sprinting);

        if (Controllable)
            GetInputs();
    }

    void FixedUpdate()
    {
        MoveBody();
    }
}