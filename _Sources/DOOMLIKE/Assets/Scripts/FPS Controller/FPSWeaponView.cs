namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    public class FPSWeaponView : MonoBehaviour
    {
        public delegate void ShootAnimationEventHandler();

        public event ShootAnimationEventHandler ShootAnimationOver;
        public event ShootAnimationEventHandler ShootFrame;
        public event ShootAnimationEventHandler ReloadFrame;

        // Animation event.
        public void OnShootFrame()
        {
            ShootFrame();
        }

        // Animation event.
        public void OnShootAnimationOver()
        {
            ShootAnimationOver();
        }

        // Animation event.
        public void OnReloadFrame()
        {
            ReloadFrame();
        }

        public void Display(bool state)
        {
            gameObject.SetActive(state);
        }
    }
}