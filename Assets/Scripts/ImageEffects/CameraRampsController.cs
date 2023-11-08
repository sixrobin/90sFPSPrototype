namespace Doomlike
{
    using UnityEngine;

    public class CameraRampsController : MonoBehaviour
    {
        [SerializeField] private UnityStandardAssets.ImageEffects.Grayscale _deathGrayscale = null;
        [SerializeField] private UnityStandardAssets.ImageEffects.Grayscale _blackScreenGrayscale = null;
        //[SerializeField] private UnityStandardAssets.ImageEffects.Grayscale _nightVisionGrayscale = null;

        public void FadeToDeathGrayscale(float delay, float desaturationDur, float pauseDur, float fadeToBlackDur, System.Action callback = null)
        {
            StartCoroutine(FadeToDeathGrayscaleCoroutine(delay, desaturationDur, pauseDur, fadeToBlackDur, callback));
        }

        public void FadeToWhiteScreenGrayscale(float delay, float fadeToWhiteDur, float delayAfter, System.Action callback = null)
        {
            StartCoroutine(FadeToWhitScreenGrayscaleCoroutine(delay, fadeToWhiteDur, delayAfter, callback));
        }

        private System.Collections.IEnumerator FadeToDeathGrayscaleCoroutine(float delay, float desaturationDur, float pauseDur, float fadeToBlackDur, System.Action callback = null)
        {
            _deathGrayscale.enabled = true;
            yield return RSLib.Yield.SharedYields.WaitForSeconds(delay);

            for (float t = 0f; t < 1f; t += Time.deltaTime / desaturationDur)
            {
                _deathGrayscale.weight = RSLib.EasingCurves.Easing.Ease(t, RSLib.EasingCurves.Curve.OutCubic);
                yield return null;
            }

            _deathGrayscale.weight = 1f;

            yield return RSLib.Yield.SharedYields.WaitForSeconds(pauseDur);

            float initRampOffset = _deathGrayscale.rampOffset;
            for (float t = 0f; t < 1f; t += Time.deltaTime / fadeToBlackDur)
            {
                _deathGrayscale.rampOffset = RSLib.Maths.Maths.Normalize(
                    RSLib.EasingCurves.Easing.Ease(t, RSLib.EasingCurves.Curve.InExpo),
                    0f,
                    1f,
                    initRampOffset,
                    -1f);

                yield return null;
            }

            _deathGrayscale.rampOffset = -1f;
            callback?.Invoke();
        }

        private System.Collections.IEnumerator FadeToWhitScreenGrayscaleCoroutine(float delay, float fadeToWhiteDur, float delayAfter, System.Action callback = null)
        {
            _blackScreenGrayscale.enabled = true;
            yield return RSLib.Yield.SharedYields.WaitForSeconds(delay);

            for (float t = 0f; t < 1f; t += Time.deltaTime / fadeToWhiteDur)
            {
                _blackScreenGrayscale.weight = RSLib.EasingCurves.Easing.Ease(t, RSLib.EasingCurves.Curve.InOutSine);
                yield return null;
            }

            _blackScreenGrayscale.weight = 1f;
            yield return RSLib.Yield.SharedYields.WaitForSeconds(delayAfter);

            callback?.Invoke();
        }
    }
}