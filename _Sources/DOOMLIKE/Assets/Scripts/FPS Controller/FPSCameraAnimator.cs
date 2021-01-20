namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    [RequireComponent(typeof(Animator))]
    public class FPSCameraAnimator : FPSComponent
    {
        private const string ANM_PARAM_DEATH = "Death";
        private const string ANM_PARAM_HURT = "Hurt";

        private Animator _animator;

        // Animation event.
        public void OnDeathAnimationHitGround(float trauma)
        {
            FPSMaster.FPSCameraShake.AddTrauma(trauma);
        }

        public void PlayDeathAnimation()
        {
            _animator.SetTrigger(ANM_PARAM_DEATH);
        }

        public void PlayHurtAnimation()
        {
            _animator.SetTrigger(ANM_PARAM_HURT);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}