namespace Doomlike.AI
{
    using UnityEngine;

    public class DummyAttackController : MonoBehaviour
    {
        [SerializeField] private float _hitRadius = 1f;
        [SerializeField] private Vector3 _localOffset = Vector3.zero;
        [SerializeField] private int _damages = 25;
        [SerializeField] private float _hitTrauma = 0.5f;

        private Collider _fpsHealthSystemCollider;
        private FPSSystem.FPSHealthSystem _fpsHealthSystem;

        public DummyController DummyController { get; private set; }

        public void Attack()
        {
            if (DummyController.DebugModeOn)
                ConsoleProLogger.Log(DummyController, "Attack.", gameObject);

            Vector3 atkPos = transform.position
                + transform.forward * _localOffset.z
                + transform.up * _localOffset.y
                + transform.right * _localOffset.x;

            Collider[] _atkResult = Physics.OverlapSphere(atkPos, _hitRadius);
            if (_atkResult.Length == 0)
                return;

            for (int i = _atkResult.Length - 1; i >= 0; --i)
            {
                FPSSystem.FPSHealthSystem fpsHealthSystem = null;
                if (_atkResult[i] == _fpsHealthSystemCollider || _atkResult[i].TryGetComponent(out fpsHealthSystem))
                {
                    if (fpsHealthSystem != null)
                        _fpsHealthSystem = fpsHealthSystem;

                    _fpsHealthSystem.Damage(_damages, _hitTrauma);
                    if (_fpsHealthSystemCollider == null)
                        _fpsHealthSystemCollider = _atkResult[i];

                    break;
                }
            }
        }

        public void SetDummyController(DummyController dummyCtrl)
        {
            DummyController = dummyCtrl;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            Vector3 atkPos = transform.position
                + transform.forward * _localOffset.z
                + transform.up * _localOffset.y
                + transform.right * _localOffset.x;

            Gizmos.DrawWireSphere(atkPos, _hitRadius);
        }
    }
}