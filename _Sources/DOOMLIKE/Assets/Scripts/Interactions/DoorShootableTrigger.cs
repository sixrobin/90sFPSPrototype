namespace Doomlike
{
    using UnityEngine;

    public class DoorShootableTrigger : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Door _door = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.1f;

        public float TraumaOnShot => _traumaOnShot;

        public void OnShot(Vector3 point)
        {
            _door.ShootTrigger();
            gameObject.SetActive(false);
        }
    }
}