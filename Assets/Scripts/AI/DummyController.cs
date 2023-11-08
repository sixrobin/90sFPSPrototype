namespace Doomlike.AI
{
    using Doomlike.Tools;
    using RSLib.Extensions;
    using RSLib.Maths;
    using UnityEngine;

    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    public class DummyController : MonoBehaviour, FPSSystem.IFPSShootable, IConsoleProLoggable
    {
        [Header("REFERENCES")]
        [SerializeField] private DummyAttackController _atkController = null;
        [SerializeField] private Transform _target = null;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private GameObject _bloodSplashPrefab = null;

        [Header("MOVEMENT")]
        [SerializeField] private float _walkSpeed = 0.5f;
        [SerializeField] private float _runSpeed = 1.5f;
        [SerializeField] private float _speedDampingTime = 0.1f;

        [Header("DETECTION")]
        [SerializeField] private float _detectionDist = 4f;
        [SerializeField] private float _startRunDist = 3f;
        [SerializeField] private float _atkDist = 0.75f;
        [SerializeField] private float _losePlayerDist = 5.5f;
        [SerializeField] private LayerMask _playerSightMask = 1;

        [Header("HEALTH")]
        [SerializeField] private int _initHealth = 100;

        [Header("MISC")]
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.2f;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;
        [SerializeField] private bool _dbgModeOn = true;
        [SerializeField] private float _dbgDist = 9f;
        [SerializeField] private Transform _dbgStateTextPivot = null;
        [SerializeField] private LineRenderer _pathView = null;

        private UnityEngine.AI.NavMeshAgent _navMeshAgent;
        private AIState _currState = AIState.Idle;
        private float _currPlayerDistSqr = Mathf.Infinity;
        private float _currSpeed;
        private float _refSpeed;

        public enum AIState
        {
            None,
            Idle,
            WalkToPlayer,
            RunToPlayer,
            Attack,
            Hurt,
            Death
        }

        public RSLib.HealthSystem HealthSystem { get; private set; }

        public float TraumaOnShot => _traumaOnShot;

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public bool DbgModeOn => _dbgModeOn;

        public string ConsoleProPrefix => "Dummy";

        public bool ConsoleProMuted => _logsMuted;

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            if (_currState == AIState.Death)
                return;

            if (Manager.ReferencesHub.FPSMaster.DbgGodMode)
                HealthSystem.Kill(); // Damages should be passed in as an OnShot() method argument.
            else
                HealthSystem.Damage(26); // TMP hard coded value.

            if (HealthSystem.IsDead)
                SetState(AIState.Death);
            else if (_currState != AIState.Hurt)
                SetState(AIState.Hurt);

            Transform bloodSplashInstance = Instantiate(_bloodSplashPrefab, shotDatas.Point, Quaternion.identity).transform;
            bloodSplashInstance.forward = transform.position - _target.position;
        }

        // Animation event.
        public void OnAttackFrame()
        {
            if (_dbgModeOn)
                this.Log($"<b>{transform.name}</b> attacking.", gameObject);

            _atkController.Attack();
        }

        // Animation event.
        public void OnAttackAnimationOver()
        {
            SetState(AIState.RunToPlayer);
        }

        // Animation event.
        public void OnHurtAnimationOver()
        {
            SetState(AIState.WalkToPlayer);
        }

        private bool CanReachTarget()
        {
            UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
            _navMeshAgent.CalculatePath(_target.position, path);
            return path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
        }

        private void SetState(AIState newState)
        {
            if (_currState == AIState.Death)
                return;

            this.Log($"Setting state to {newState}.");
            _currState = newState;

            switch (_currState)
            {
                case AIState.Idle:
                {
                    _currSpeed = 0f;
                    break;
                }

                case AIState.WalkToPlayer:
                {
                    _navMeshAgent.SetDestination(_target.position);
                    break;
                }

                case AIState.RunToPlayer:
                {
                    _navMeshAgent.SetDestination(_target.position);
                    break;
                }

                case AIState.Attack:
                {
                    LookAtPlayer();
                    _animator.SetTrigger($"Atk{Random.Range(0, 2)}");
                    _currSpeed = 0f;
                    break;
                }

                case AIState.Hurt:
                {
                    // Stop navmesh.
                    _animator.SetTrigger("Hurt");
                    _currSpeed = 0f;
                    break;
                }

                case AIState.Death:
                {
                    _collider.enabled = false;
                    _navMeshAgent.enabled = false;
                    _pathView.enabled = false;
                    LookAtPlayer();

                    foreach (AnimatorControllerParameter anmParam in _animator.parameters)
                        if (anmParam.type == AnimatorControllerParameterType.Trigger)
                            _animator.ResetTrigger(anmParam.name);

                    _animator.SetTrigger("Death");
                    break;
                }
            }

            if (_currState != AIState.Death)
                _navMeshAgent.isStopped = _currState == AIState.Attack || _currState == AIState.Hurt;
        }

        private void Act()
        {
            switch (_currState)
            {
                case AIState.Idle:
                {
                    // Detect player = sight, ear or distance.
                    if (_currPlayerDistSqr <= _detectionDist.Sqr() && CanReachTarget())
                        SetState(AIState.WalkToPlayer);

                    break;
                }

                case AIState.WalkToPlayer:
                {
                    if (_currPlayerDistSqr <= _startRunDist.Sqr())
                        SetState(AIState.RunToPlayer);
                    else if (_currPlayerDistSqr > _losePlayerDist.Sqr() || !CanReachTarget())
                        SetState(AIState.Idle);

                    break;
                }

                case AIState.RunToPlayer:
                {
                    if (_currPlayerDistSqr <= _atkDist.Sqr())
                        SetState(AIState.Attack);
                    if (_currPlayerDistSqr > _startRunDist.Sqr())
                        SetState(AIState.WalkToPlayer);

                    break;
                }

                default:
                    break; // Hurt/Atk/Death, nothing to do.
            }
        }

        private bool HasPlayerInSight()
        {
            // Requires optimization/caching.
            return Physics.Raycast(transform.position, _target.position - transform.position, out RaycastHit hit, Mathf.Infinity, _playerSightMask)
                && hit.collider.GetComponent<FPSSystem.FPSController>() || hit.collider.GetComponent<FPSSystem.FPSHealthSystem>();
        }

        private void EvaluateSpeed()
        {
            float targetSpeed = 0f;
            if (_currState == AIState.WalkToPlayer)
                targetSpeed = _walkSpeed;
            else if (_currState == AIState.RunToPlayer)
                targetSpeed = _runSpeed;

            _currSpeed = Mathf.SmoothDamp(_currSpeed, targetSpeed, ref _refSpeed, _speedDampingTime);
            _navMeshAgent.speed = _currSpeed;
        }

        private void UpdateDistanceToPlayer()
        {
            _navMeshAgent.SetDestination(_target.position);
            _currPlayerDistSqr = _navMeshAgent.hasPath ? _navMeshAgent.ComputeRemainingDistanceSqr() : Mathf.Infinity;
        }

        private void AdjustAnimatorMoveSpeed()
        {
            switch (_currState)
            {
                case AIState.Idle:
                case AIState.Attack:
                case AIState.Hurt:
                    _animator.SetBool("Moving", false);
                    _animator.SetFloat("MoveSpeed", 0f);
                    break;

                case AIState.WalkToPlayer:
                case AIState.RunToPlayer:
                    _animator.SetBool("Moving", true);
                    _animator.SetFloat("MoveSpeed", RSLib.Maths.Maths.Normalize01Clamped(_currSpeed, _walkSpeed, _runSpeed));
                    break;
            }
        }

        private void LookAtPlayer()
        {
            transform.forward = (_target.position - transform.position).WithY(0f);
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            _atkController.SetDummyController(this);
            HealthSystem = new RSLib.HealthSystem(_initHealth);

            if (_losePlayerDist < _detectionDist)
                this.Log($"LosePlayerDist ({_losePlayerDist}) is less than DetectionDist({_detectionDist}).");
        }

        private void Update()
        {
            if (_currState == AIState.Death)
                return;

            UpdateDistanceToPlayer();
            EvaluateSpeed();
            AdjustAnimatorMoveSpeed();
            Act();

            _pathView.enabled = _dbgModeOn && Manager.DebugManager.DbgViewOn;
            if (_pathView.enabled)
            {
                _pathView.positionCount = _navMeshAgent.path.corners.Length;
                for (int i = 0; i < _navMeshAgent.path.corners.Length; ++i)
                    _pathView.SetPosition(i, _navMeshAgent.path.corners[i].AddY(0.1f));
            }
        }

        private void OnGUI()
        {
            if (!_dbgModeOn || !Manager.DebugManager.DbgViewOn || _currPlayerDistSqr > _dbgDist.Sqr() || _currState == AIState.Death)
                return;

            Camera mainCamera = Camera.main;
            if ((mainCamera.transform.position - transform.position).sqrMagnitude > 16f)
                return;

            Vector3 worldPos = mainCamera.WorldToScreenPoint(_dbgStateTextPivot.position);
            if (worldPos.z < 0f)
                return;

            GUIStyle dbgStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 9,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, 1f)
                }
            };

            worldPos.y = Screen.height - worldPos.y;
            GUI.Label(new Rect(worldPos.x, worldPos.y, 200f, 100f), $"State: {_currState}", dbgStyle);
            GUI.Label(new Rect(worldPos.x, worldPos.y + 10f, 200f, 100f), $"Sight: {HasPlayerInSight()}", dbgStyle);
            GUI.Label(new Rect(worldPos.x, worldPos.y + 20f, 200f, 100f), $"Reach: {CanReachTarget()}", dbgStyle);
        }
    }
}