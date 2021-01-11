namespace Doomlike
{
    using UnityEngine;

    public class CameraRampsController : MonoBehaviour
    {
        [SerializeField] private UnityStandardAssets.ImageEffects.Grayscale _deathGrayscale = null;
        [SerializeField] private UnityStandardAssets.ImageEffects.Grayscale _nightVisionGrayscale = null;

        public void FadeToDeathGrayscale(float delay, float desaturationDur, float pauseDur, float fadeToBlackDur)
        {
            StartCoroutine(FadeToDeathGrayscaleCoroutine(delay, desaturationDur, pauseDur, fadeToBlackDur));
        }

        private System.Collections.IEnumerator FadeToDeathGrayscaleCoroutine(float delay, float desaturationDur, float pauseDur, float fadeToBlackDur)
        {
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
        }
    }
}