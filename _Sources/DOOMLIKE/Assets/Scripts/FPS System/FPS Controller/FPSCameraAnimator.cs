namespace Doomlike.FPSSystem
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

        public void PlayHurtAnimation(float delay = 0f)
        {
            if (delay == 0f)
                _animator.SetTrigger(ANM_PARAM_HURT);
            else
                StartCoroutine(PlayHurtAnimationDelayed(delay));
        }

        private System.Collections.IEnumerator PlayHurtAnimationDelayed(float delay)
        {
            yield return RSLib.Yield.SharedYields.WaitForSeconds(delay);
            _animator.SetTrigger(ANM_PARAM_HURT);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}