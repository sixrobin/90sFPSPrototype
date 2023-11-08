namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    public class SoldierController : OutlinedInteraction, FPSSystem.IFPSShootable, IConsoleProLoggable
    {
        private const string ANM_PARAM_HURT = "Hurt";
        private const string ANM_PARAM_SHOOT = "Shoot";
        private const string ANM_PARAM_AIM = "Aim";
        private const string ANM_PARAM_LEAN = "Lean";

        [SerializeField] private DialogueSystem.Dialogue _initDialogue = null;

        [SerializeField] private Animator _animator = null;
        [SerializeField, Min(0.5f)] private float _shootsInterval = 2f;
        [SerializeField] private GameObject _bloodParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.15f;

        [SerializeField] private bool _consoleProMuted = false;

        private Vector3 _initForward;
        private Transform _target;
        private int _hurtCount;

        private SoldierState _currState = SoldierState.Idle;
        private float _currStateTimer = 0f;

        private DialogueSystem.DialoguePlaylist _dialoguePlaylist;

        public enum SoldierState
        {
            None,
            Idle,
            Hurt,
            Aim,
            Shoot
        }

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public float TraumaOnShot => _traumaOnShot;

        public bool ConsoleProMuted => _consoleProMuted;

        public string ConsoleProPrefix => "Soldier";

        public override void Focus()
        {
            if (_currState == SoldierState.Idle)
                base.Focus();
        }

        public override void Interact()
        {
            if (_currState == SoldierState.Idle)
            {
                base.Interact();
                Manager.ReferencesHub.DialogueController?.Play(_dialoguePlaylist.Next());
            }
        }

        // Animation event.
        public void OnHurtAnimationOver()
        {
            SetState(SoldierState.Aim);
        }

        // Animation event.
        public void OnShootFrame()
        {
            if (_hurtCount == 1)
            {
                int dmg = Manager.ReferencesHub.FPSMaster.FPSHealthSystem.HealthSystem.Health > 10
                    ? Manager.ReferencesHub.FPSMaster.FPSHealthSystem.HealthSystem.Health - 10
                    : 0;

                Manager.ReferencesHub.FPSMaster.FPSHealthSystem.Damage(dmg, 0.6f);
            }
            else
            {
                Manager.ReferencesHub.FPSMaster.FPSHealthSystem.Kill(0.5f);
            }
        }

        // Animation event.
        public void OnShootAnimationOver()
        {
            SetState(SoldierState.Idle);
        }

        public void OverrideNextDialogue(DialogueSystem.Dialogue nextDialogue)
        {
            _dialoguePlaylist.OverrideNextDialogue(nextDialogue);
        }

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            if (_currState != SoldierState.Idle)
                return;

            SetState(SoldierState.Hurt);
            _target = Manager.ReferencesHub.FPSMaster.FPSController.transform;

            _animator.SetTrigger(ANM_PARAM_HURT);
            Instantiate(_bloodParticles, shotDatas.Point, _bloodParticles.transform.rotation);
        }

        protected override void Awake()
        {
            base.Awake();
            _initForward = transform.forward;
            _dialoguePlaylist = new DialogueSystem.DialoguePlaylist(_initDialogue);
        }

        private void SetState(SoldierState newState)
        {
            _currState = newState;
            _currStateTimer = 0f;

            this.Log($"Setting state to {newState}.");

            switch (_currState)
            {
                case SoldierState.Idle:
                    _animator.SetTrigger(ANM_PARAM_LEAN);
                    transform.forward = _initForward;
                    SetInteractionAvailability(true);
                    break;

                case SoldierState.Hurt:
                    _hurtCount++;
                    DisallowInteraction();
                    break;

                case SoldierState.Aim:
                    _animator.SetTrigger(ANM_PARAM_AIM);
                    break;

                case SoldierState.Shoot:
                    _animator.SetTrigger(ANM_PARAM_SHOOT);
                    break;
            }
        }

        private void Update()
        {
            _currStateTimer += Time.deltaTime;

            switch (_currState)
            {
                case SoldierState.Idle:
                case SoldierState.Hurt:
                    break;

                case SoldierState.Aim:
                    transform.forward = (_target.position - transform.position).WithY(0f);
                    if (_currStateTimer > _shootsInterval)
                        SetState(SoldierState.Shoot);
                    break;

                case SoldierState.Shoot:
                    transform.forward = (_target.position - transform.position).WithY(0f);
                    break;
            }
        }
    }
}