namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    [RequireComponent(typeof(Animator))]
    public class FPSCameraAnimator : FPSComponent
    {
        private Animator _animator;

        // Animation event.
        public void OnDeathAnimationHitGround(float trauma)
        {
            FPSMaster.FPSCameraShake.AddTrauma(trauma);
        }

        public void PlayDeathAnimation()
        {
            _animator.SetTrigger("Death");
        }

        public void PlayHurtAnimation()
        {
            _animator.SetTrigger("Hurt");
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}