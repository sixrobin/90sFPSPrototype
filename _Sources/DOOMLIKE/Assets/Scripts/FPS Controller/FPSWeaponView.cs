using UnityEngine;

public class FPSWeaponView : MonoBehaviour
{
    public delegate void ShootAnimationEventHandler();

    public event ShootAnimationEventHandler ShootAnimationOver;
    public event ShootAnimationEventHandler ShootFrame;

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

    public void Display(bool state)
    {
        gameObject.SetActive(state);
    }
}