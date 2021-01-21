namespace Doomlike
{
    using UnityEngine;

    public class GameObjectEnabler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _objects = null;
        [SerializeField, Min(-1f)] private float _distToEnable = -1f;

        public void Enable()
        {
            for (int i = _objects.Length - 1; i >= 0; --i)
                _objects[i].SetActive(true);
        }

        public void EnableIfPlayerInDistance()
        {
            if (_distToEnable > -1f || (Manager.ReferencesHub.FPSMaster.FPSController.transform.position - transform.position).sqrMagnitude < _distToEnable * _distToEnable)
                Enable();
        }

        private void OnDrawGizmosSelected()
        {
            if (_distToEnable == -1f)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _distToEnable);
        }
    }
}