namespace Doomlike.AI
{
    using RSLib.Extensions;
    using RSLib.Maths;
    using UnityEngine;

    public class DummyController : MonoBehaviour, FPSCtrl.IFPSShootable, IConsoleProLoggable
    {
        [Header("REFERENCES")]
        [SerializeField] private FPSCtrl.FPSController _fpsController = null;
        [SerializeField] private UnityEngine.AI.NavMeshAgent _navMeshAgent = null;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private GameObject _bloodSplashPrefab = null;

        [Header("MOVEMENT")]
        [SerializeField] private float _walkSpeed = 0.5f;
        [SerializeField] private float _runSpeed = 1.5f;
        [SerializeField] private float _speedDampingTime = 0.1f;

        [Header("DETECTION")]
        [SerializeField] private float _detectionDist = 4f;
        [SerializeField] private float _startRunDist = 3f;
        [SerializeField] private float _atkDist = 0.75f;

        [Header("MISC")]
        [SerializeField] private float _traumaOnShot = 0.2f;

        [Header("DEBUG")]
        [SerializeField] private bool _dbgModeOn = true;
        [SerializeField] private float _dbgDist = 9f;
        [SerializeField] private Transform _dbgStateTextPivot = null;
        [SerializeField] private LineRenderer _pathView = null;

        private AIState _currState = AIState.Idle;
        private float _currPlayerDistSqr;
        private float _currSpeed;
        private float _refSpeed;

        public enum AIState
        {
            None,
            Idle,
            WalkToPlayer,
            RunToPlayer,
            Attack,
            Hurt
        }

        public string ConsoleProPrefix => "Dummy Controller";

        public float TraumaOnShot => _traumaOnShot;

        public void OnShot(Vector3 point)
        {
            // TODO: Damages.

            if (_currState != AIState.Hurt)
                SetState(AIState.Hurt);

            Transform bloodSplashInstance = Instantiate(_bloodSplashPrefab, point, Quaternion.identity).transform;
            bloodSplashInstance.forward = transform.position - _fpsController.transform.position;
        }

        // Animation event.
        public void OnAttackFrame()
        {
            if (_dbgModeOn)
                ConsoleProLogger.Log(this, $"<b>{transform.name}</b> attacking.", gameObject);
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

        private void SetState(AIState newState)
        {
            switch (newState)
            {
                case AIState.Idle:
                {
                    _currSpeed = 0f;
                    break;
                }

                case AIState.WalkToPlayer:
                {
                    _navMeshAgent.SetDestination(_fpsController.transform.position);
                    break;
                }

                case AIState.RunToPlayer:
                {
                    _navMeshAgent.SetDestination(_fpsController.transform.position);
                    break;
                }

                case AIState.Attack:
                {
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
            }

            _currState = newState;
            _navMeshAgent.isStopped = _currState == AIState.Attack || _currState == AIState.Hurt;
        }

        private void Act()
        {
            switch (_currState)
            {
                case AIState.Idle:
                {
                    if (_currPlayerDistSqr <= _detectionDist.Sqr())
                        SetState(AIState.WalkToPlayer);

                    break;
                }

                case AIState.WalkToPlayer:
                {
                    if (_currPlayerDistSqr <= _startRunDist.Sqr())
                        SetState(AIState.RunToPlayer);
                    else if (_currPlayerDistSqr > _detectionDist.Sqr())
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

                case AIState.Attack:
                {
                    // Attack animation running naturally.
                    break;
                }
            }
        }

        private bool HasPlayerInSight()
        {
            if (Physics.Raycast(transform.position.WithY(0.5f), _fpsController.transform.position - transform.position, out RaycastHit hit, Mathf.Infinity))
                return hit.collider.GetComponent<FPSCtrl.FPSController>();
            return false;
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
            _navMeshAgent.SetDestination(_fpsController.transform.position);
            _currPlayerDistSqr = _navMeshAgent.ComputeRemainingDistanceSqr();
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

        private void Update()
        {
            UpdateDistanceToPlayer();
            EvaluateSpeed();
            AdjustAnimatorMoveSpeed();
            Act();

            _pathView.enabled = _dbgModeOn;
            if (_dbgModeOn)
            {
                _pathView.positionCount = _navMeshAgent.path.corners.Length;
                for (int i = 0; i < _navMeshAgent.path.corners.Length; ++i)
                    _pathView.SetPosition(i, _navMeshAgent.path.corners[i].AddY(0.1f));
            }
        }

        private void OnGUI()
        {
            if (!_dbgModeOn || _currPlayerDistSqr > _dbgDist.Sqr())
                return;

            Vector3 worldPos = Camera.main.WorldToScreenPoint(_dbgStateTextPivot.position);
            if (worldPos.z < 0f)
                return;

            GUIStyle dbgStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, 1f)
                }
            };

            worldPos.y = Screen.height - worldPos.y;
            GUI.Label(new Rect(worldPos.x, worldPos.y, 200f, 100f), _currState.ToString(), dbgStyle);
            GUI.Label(new Rect(worldPos.x, worldPos.y + 15f, 200f, 100f), $"Sight: {HasPlayerInSight()}", dbgStyle);
        }
    }
}