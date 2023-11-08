namespace Doomlike.FPSSystem
{
    using UnityEngine;

    public class FPSWeaponView : MonoBehaviour
    {
        [SerializeField] private GameObject _weaponView = null;

        public delegate void ShootAnimationEventHandler();

        public event ShootAnimationEventHandler ShootAnimationOver;
        public event ShootAnimationEventHandler ReloadAnimationOver;
        public event ShootAnimationEventHandler ShootFrame;
        public event ShootAnimationEventHandler ReloadFrame;

        // Animation event.
        public void OnShootAnimationOver()
        {
            ShootAnimationOver();
        }

        // Animation event.
        public void OnReloadAnimationOver()
        {
            ReloadAnimationOver();
        }

        // Animation event.
        public void OnShootFrame()
        {
            ShootFrame();
        }

        // Animation event.
        public void OnReloadFrame()
        {
            ReloadFrame();
        }

        public void Display(bool state)
        {
            // We don't want to toggle this.gameObject because we want the animator to be running continously (for events).
            _weaponView.SetActive(state);
        }
    }
}