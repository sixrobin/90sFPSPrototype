namespace RSLib
{
    using RSLib.EasingCurves;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    public class PostProcessController : RSLib.Framework.Singleton<PostProcessController>
    {
        private IEnumerator _bloomCoroutine;
        private IEnumerator _chromaticAberrationCoroutine;
        private IEnumerator _depthOfFieldCoroutine;
        private IEnumerator _grainCoroutine;
        private IEnumerator _lensDistorsionCoroutine;
        private IEnumerator _vignetteCoroutine;

        private bool _bloomFading;
        private bool _chromaticAberrationFading;
        private bool _depthOfFieldFading;
        private bool _grainFading;
        private bool _lensDistorsionFading;
        private bool _vignetteFading;

        private bool AnyEffectFading => _bloomFading
            || _chromaticAberrationFading
            || _depthOfFieldFading
            || _grainFading
            || _lensDistorsionFading
            || _vignetteFading;

        /// <summary>
        /// The volume that will be affected.
        /// </summary>
        [SerializeField] private PostProcessVolume _volume = null;

        public bool TimeScaleDependent = true;

        /// <summary>
        /// Checks if the volume has an effect and if it is enabled.
        /// </summary>
        /// <typeparam name="T"> Type of the checked effect. </typeparam>
        /// <returns> True if the volume has the effect and if it is enabled. </returns>
        private bool CheckEffectEnabled<T>() where T : PostProcessEffectSettings
        {
            if (!_volume.profile.TryGetSettings(out T effect))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a " + typeof(T).Name + " effect.", _volume.gameObject);
                return false;
            }
            if (!effect.active || !effect.enabled)
            {
                Debug.LogWarning("PostProcessController WARNING: Volume's " + effect.GetType().Name + " effect is disabled.", _volume.gameObject);
                return false;
            }

            return true;
        }

        #region General settings

        /// <summary>
        /// Changes the volume that will be affected.
        /// </summary>
        /// <param name="newVolume"> The new volume. </param>
        public void ChangeTargetVolume(PostProcessVolume newVolume)
        {
            if (AnyEffectFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Can not change volume while a fade is occuring.");
                return;
            }

            _volume = newVolume;
        }

        /// <summary>
        /// Sets the volume as global or not.
        /// </summary>
        /// <param name="state"> New global state. </param>
        public void SetAsGlobal(bool state)
        {
            _volume.isGlobal = state;
        }

        /// <summary>
        /// Sets the volume weight (opacity clamped between 0 and 1).
        /// </summary>
        /// <param name="weight"> The new weight. </param>
        public void SetWeight(float weight)
        {
            _volume.weight = weight;
        }

        /// <summary>
        /// Sets the volume priority.
        /// </summary>
        /// <param name="priority"> The new priority. </param>
        public void SetPriority(float priority)
        {
            _volume.priority = priority;
        }

        #endregion

        #region Fades

        #region Bloom

        private IEnumerator FadeBloom(float intensity, float threshold, float softKnee, Color color, float fadeDuration, Curve fadeCurve)
        {
            _bloomFading = true;
            _volume.profile.TryGetSettings(out Bloom bloom);

            float initIntensity = bloom.intensity.value;
            float initThreshold = bloom.threshold.value;
            float initSoftKnee = bloom.softKnee.value;
            Color initColor = bloom.color.value;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / fadeDuration)
            {
                bloom.intensity.Override(Mathf.Lerp(initIntensity, intensity, Easing.Ease(t, fadeCurve)));
                bloom.threshold.Override(Mathf.Lerp(initThreshold, threshold, Easing.Ease(t, fadeCurve)));
                bloom.softKnee.Override(Mathf.Lerp(initSoftKnee, softKnee, Easing.Ease(t, fadeCurve)));
                bloom.color.Override(Color.Lerp(initColor, color, Easing.Ease(t, fadeCurve)));

                yield return null;
            }

            OverrideBloomValues(intensity, threshold, softKnee, color);
            _bloomFading = false;
        }

        private IEnumerator BlinkBloom(float intensity, float threshold, float softKnee, Color color, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            _volume.profile.TryGetSettings(out Bloom bloom);

            float initIntensity = bloom.intensity.value;
            float initThreshold = bloom.threshold.value;
            float initSoftKnee = bloom.softKnee.value;
            Color initColor = bloom.color.value;

            yield return StartCoroutine(FadeBloom(intensity, threshold, softKnee, color, inDuration, inCurve));
            _bloomFading = true;

            if (TimeScaleDependent) yield return new WaitForSeconds(waitDuration);
            else yield return new WaitForSecondsRealtime(waitDuration);

            yield return StartCoroutine(FadeBloom(initIntensity, initThreshold, initSoftKnee, initColor, outDuration, outCurve));
        }

        public void FadeBloomToValues(float intensity, float threshold, float softKnee, Color color, float fadeDuration, Curve fadeCurve)
        {
            if (_bloomFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Bloom is already fading.");
                return;
            }
            if (!CheckEffectEnabled<Bloom>())
                return;

            _bloomCoroutine = FadeBloom(intensity, threshold, softKnee, color, fadeDuration, fadeCurve);
            StartCoroutine(_bloomCoroutine);
        }

        public void BlinkBloomWithValues(float intensity, float threshold, float softKnee, Color color, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            if (_bloomFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Bloom is already fading.");
                return;
            }
            if (!CheckEffectEnabled<Bloom>())
                return;

            _bloomCoroutine = BlinkBloom(intensity, threshold, softKnee, color, inDuration, waitDuration, outDuration, inCurve, outCurve);
            StartCoroutine(_bloomCoroutine);
        }

        #endregion

        #region Chromatic aberration

        private IEnumerator FadeChromaticAberration(float intensity, float fadeDuration, Curve fadeCurve)
        {
            _chromaticAberrationFading = true;
            _volume.profile.TryGetSettings(out ChromaticAberration chromaticAberration);

            float initIntensity = chromaticAberration.intensity.value;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / fadeDuration)
            {
                chromaticAberration.intensity.Override(Mathf.Lerp(initIntensity, intensity, Easing.Ease(t, fadeCurve)));

                yield return null;
            }

            chromaticAberration.intensity.Override(intensity);
            _chromaticAberrationFading = false;
        }

        private IEnumerator BlinkChromaticAberration(float intensity, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            _volume.profile.TryGetSettings(out ChromaticAberration chromaticAberration);

            float initIntensity = chromaticAberration.intensity.value;

            yield return StartCoroutine(FadeChromaticAberration(intensity, inDuration, inCurve));
            _chromaticAberrationFading = true;

            if (TimeScaleDependent) yield return new WaitForSeconds(waitDuration);
            else yield return new WaitForSecondsRealtime(waitDuration);

            yield return StartCoroutine(FadeChromaticAberration(initIntensity, outDuration, outCurve));
        }

        public void FadeChromaticAberrationToValues(float intensity, float fadeDuration, Curve fadeCurve)
        {
            if (_chromaticAberrationFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Chromatic aberration is already fading.");
                return;
            }
            if (!CheckEffectEnabled<ChromaticAberration>())
                return;

            _chromaticAberrationCoroutine = FadeChromaticAberration(intensity, fadeDuration, fadeCurve);
            StartCoroutine(_chromaticAberrationCoroutine);
        }

        public void BlinkChromaticAberrationWithValues(float intensity, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            if (_chromaticAberrationFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Chromatic aberration is already fading.");
                return;
            }
            if (!CheckEffectEnabled<ChromaticAberration>())
                return;

            _chromaticAberrationCoroutine = BlinkChromaticAberration(intensity, inDuration, waitDuration, outDuration, inCurve, outCurve);
            StartCoroutine(_chromaticAberrationCoroutine);
        }

        #endregion

        #region Depth of field

        private IEnumerator FadeDepthOfField(float focusDist, float aperture, float focalLength, float fadeDuration, Curve fadeCurve)
        {
            _depthOfFieldFading = true;
            _volume.profile.TryGetSettings(out DepthOfField dof);

            float initFocusDist = dof.focusDistance.value;
            float initAperture = dof.aperture.value;
            float initFocalLength = dof.focalLength.value;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / fadeDuration)
            {
                dof.focusDistance.Override(Mathf.Lerp(initFocusDist, focusDist, Easing.Ease(t, fadeCurve)));
                dof.aperture.Override(Mathf.Lerp(initAperture, aperture, Easing.Ease(t, fadeCurve)));
                dof.focalLength.Override(Mathf.Lerp(initFocalLength, focalLength, Easing.Ease(t, fadeCurve)));

                yield return null;
            }

            OverrideDepthOfFieldValues(focusDist, aperture, focalLength);
            _depthOfFieldFading = false;
        }

        private IEnumerator BlinkDepthOfField(float focusDist, float aperture, float focalLength, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            _volume.profile.TryGetSettings(out DepthOfField dof);

            float initFocusDist = dof.focusDistance.value;
            float initAperture = dof.aperture.value;
            float initFocalLength = dof.focalLength.value;

            yield return StartCoroutine(FadeDepthOfField(focusDist, aperture, focalLength, inDuration, inCurve));
            _depthOfFieldFading = true;

            if (TimeScaleDependent) yield return new WaitForSeconds(waitDuration);
            else yield return new WaitForSecondsRealtime(waitDuration);

            yield return StartCoroutine(FadeDepthOfField(initFocusDist, initAperture, initFocalLength, outDuration, outCurve));
        }

        public void FadeDepthOfFieldToValues(float focusDist, float aperture, float focalLength, float fadeDuration, Curve fadeCurve)
        {
            if (_depthOfFieldFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Depth of field is already fading.");
                return;
            }
            if (!CheckEffectEnabled<DepthOfField>())
                return;

            _depthOfFieldCoroutine = FadeDepthOfField(focusDist, aperture, focalLength, fadeDuration, fadeCurve);
            StartCoroutine(_depthOfFieldCoroutine);
        }

        public void BlinkDepthOfFieldWithValues(float focusDist, float aperture, float focalLength, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            if (_depthOfFieldFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Depth of field is already fading.");
                return;
            }
            if (!CheckEffectEnabled<DepthOfField>())
                return;

            _depthOfFieldCoroutine = BlinkDepthOfField(focusDist, aperture, focalLength, inDuration, waitDuration, outDuration, inCurve, outCurve);
            StartCoroutine(_depthOfFieldCoroutine);
        }

        #endregion

        #region Grain

        private IEnumerator FadeGrain(float intensity, float size, float luminanceContribution, float fadeDuration, Curve fadeCurve)
        {
            _grainFading = true;
            _volume.profile.TryGetSettings(out Grain grain);

            float initIntensity = grain.intensity.value;
            float initSize = grain.size.value;
            float initLuminanceContribution = grain.lumContrib.value;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / fadeDuration)
            {
                grain.intensity.Override(Mathf.Lerp(initIntensity, intensity, Easing.Ease(t, fadeCurve)));
                grain.size.Override(Mathf.Lerp(initSize, size, Easing.Ease(t, fadeCurve)));
                grain.lumContrib.Override(Mathf.Lerp(initLuminanceContribution, luminanceContribution, Easing.Ease(t, fadeCurve)));

                yield return null;
            }

            OverrideGrainValues(grain.colored.value, intensity, size, luminanceContribution);
            _grainFading = false;
        }

        private IEnumerator BlinkGrain(float intensity, float size, float luminanceContribution, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            _volume.profile.TryGetSettings(out Grain grain);

            float initIntensity = grain.intensity.value;
            float initSize = grain.size.value;
            float initLuminanceContribution = grain.lumContrib.value;

            yield return StartCoroutine(FadeGrain(intensity, size, luminanceContribution, inDuration, inCurve));
            _grainFading = true;

            if (TimeScaleDependent) yield return new WaitForSeconds(waitDuration);
            else yield return new WaitForSecondsRealtime(waitDuration);

            yield return StartCoroutine(FadeGrain(initIntensity, initSize, initLuminanceContribution, outDuration, outCurve));
        }

        public void FadeGrainToValues(float intensity, float size, float luminanceContribution, float fadeDuration, Curve fadeCurve)
        {
            if (_grainFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Grain is already fading.");
                return;
            }
            if (!CheckEffectEnabled<Grain>())
                return;

            _grainCoroutine = FadeGrain(intensity, size, luminanceContribution, fadeDuration, fadeCurve);
            StartCoroutine(_grainCoroutine);
        }

        public void BlinkGrainWithValues(float intensity, float size, float luminanceContribution, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            if (_grainFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Grain is already fading.");
                return;
            }
            if (!CheckEffectEnabled<Grain>())
                return;

            _grainCoroutine = BlinkGrain(intensity, size, luminanceContribution, inDuration, waitDuration, outDuration, inCurve, outCurve);
            StartCoroutine(_grainCoroutine);
        }

        #endregion

        #region Lens distorsion

        private IEnumerator FadeLensDistorsion(float intensity, float xMultiplier, float yMultiplier, float centerX, float centerY, float scale, float fadeDuration, Curve fadeCurve)
        {
            _lensDistorsionFading = true;
            _volume.profile.TryGetSettings(out LensDistortion lensDistorsion);

            float initIntensity = lensDistorsion.intensity.value;
            float initXMultiplier = lensDistorsion.intensityX.value;
            float initYMultiplier = lensDistorsion.intensityY.value;
            float initCenterX = lensDistorsion.centerX.value;
            float initCenterY = lensDistorsion.centerY.value;
            float initScale = lensDistorsion.scale.value;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / fadeDuration)
            {
                lensDistorsion.intensity.Override(Mathf.Lerp(initIntensity, intensity, Easing.Ease(t, fadeCurve)));
                lensDistorsion.intensityX.Override(Mathf.Lerp(initXMultiplier, xMultiplier, Easing.Ease(t, fadeCurve)));
                lensDistorsion.intensityY.Override(Mathf.Lerp(initYMultiplier, yMultiplier, Easing.Ease(t, fadeCurve)));
                lensDistorsion.centerX.Override(Mathf.Lerp(initCenterX, centerX, Easing.Ease(t, fadeCurve)));
                lensDistorsion.centerY.Override(Mathf.Lerp(initCenterY, centerY, Easing.Ease(t, fadeCurve)));
                lensDistorsion.scale.Override(Mathf.Lerp(initScale, scale, Easing.Ease(t, fadeCurve)));

                yield return null;
            }

            OverrideLensDistorsionValues(intensity, xMultiplier, yMultiplier, centerX, centerY, scale);
            _lensDistorsionFading = false;
        }

        private IEnumerator BlinkLensDistorsion(float intensity, float xMultiplier, float yMultiplier, float centerX, float centerY, float scale, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            _volume.profile.TryGetSettings(out LensDistortion lensDistorsion);

            float initIntensity = lensDistorsion.intensity.value;
            float initXMultiplier = lensDistorsion.intensityX.value;
            float initYMultiplier = lensDistorsion.intensityY.value;
            float initCenterX = lensDistorsion.centerX.value;
            float initCenterY = lensDistorsion.centerY.value;
            float initScale = lensDistorsion.scale.value;

            yield return StartCoroutine(FadeLensDistorsion(intensity, xMultiplier, yMultiplier, centerX, centerY, scale, inDuration, inCurve));
            _lensDistorsionFading = true;

            if (TimeScaleDependent) yield return new WaitForSeconds(waitDuration);
            else yield return new WaitForSecondsRealtime(waitDuration);

            yield return StartCoroutine(FadeLensDistorsion(initIntensity, initXMultiplier, initYMultiplier, initCenterX, initCenterY, initScale, outDuration, outCurve));
        }

        public void FadeLensDistorsionToValues(float intensity, float xMultiplier, float yMultiplier, float centerX, float centerY, float scale, float fadeDuration, Curve fadeCurve)
        {
            if (_lensDistorsionFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Lens distorsion is already fading.");
                return;
            }
            if (!CheckEffectEnabled<LensDistortion>())
                return;

            _lensDistorsionCoroutine = FadeLensDistorsion(intensity, xMultiplier, yMultiplier, centerX, centerY, scale, fadeDuration, fadeCurve);
            StartCoroutine(_lensDistorsionCoroutine);
        }

        public void BlinkLensDistorsionWithValues(float intensity, float xMultiplier, float yMultiplier, float centerX, float centerY, float scale, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            if (_lensDistorsionFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Lens distorsion is already fading.");
                return;
            }
            if (!CheckEffectEnabled<LensDistortion>())
                return;

            _lensDistorsionCoroutine = BlinkLensDistorsion(intensity, xMultiplier, yMultiplier, centerX, centerY, scale, inDuration, waitDuration, outDuration, inCurve, outCurve);
            StartCoroutine(_lensDistorsionCoroutine);
        }

        #endregion

        #region Vignette

        private IEnumerator FadeVignette(Color color, float intensity, float smoothness, float roundness, float fadeDuration, Curve fadeCurve)
        {
            _vignetteFading = true;
            _volume.profile.TryGetSettings(out Vignette vignette);

            Color initColor = vignette.color.value;
            float initIntensity = vignette.intensity.value;
            float initSmoothness = vignette.smoothness.value;
            float initRoundness = vignette.roundness.value;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / fadeDuration)
            {
                vignette.color.Override(Color.Lerp(initColor, color, Easing.Ease(t, fadeCurve)));
                vignette.intensity.Override(Mathf.Lerp(initIntensity, intensity, Easing.Ease(t, fadeCurve)));
                vignette.smoothness.Override(Mathf.Lerp(initSmoothness, smoothness, Easing.Ease(t, fadeCurve)));
                vignette.roundness.Override(Mathf.Lerp(initRoundness, roundness, Easing.Ease(t, fadeCurve)));

                yield return null;
            }

            OverrideVignetteValues(color, intensity, smoothness, roundness, vignette.rounded.value);
            _vignetteFading = false;
        }

        private IEnumerator BlinkVignette(Color color, float intensity, float smoothness, float roundness, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            _volume.profile.TryGetSettings(out Vignette vignette);

            Color initColor = vignette.color.value;
            float initIntensity = vignette.intensity.value;
            float initSmoothness = vignette.smoothness.value;
            float initRoundness = vignette.roundness.value;

            yield return StartCoroutine(FadeVignette(color, intensity, smoothness, roundness, inDuration, inCurve));
            _vignetteFading = true;

            if (TimeScaleDependent) yield return new WaitForSeconds(waitDuration);
            else yield return new WaitForSecondsRealtime(waitDuration);

            yield return StartCoroutine(FadeVignette(initColor, initIntensity, initSmoothness, initRoundness, outDuration, outCurve));
        }

        public void FadeVignetteToValues(Color color, float intensity, float smoothness, float roundness, float fadeDuration, Curve fadeCurve)
        {
            if (_vignetteFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Vignette is already fading.");
                return;
            }
            if (!CheckEffectEnabled<Vignette>())
                return;

            _vignetteCoroutine = FadeVignette(color, intensity, smoothness, roundness, fadeDuration, fadeCurve);
            StartCoroutine(_vignetteCoroutine);
        }

        public void BlinkVignetteWithValues(Color color, float intensity, float smoothness, float roundness, float inDuration, float waitDuration, float outDuration, Curve inCurve, Curve outCurve)
        {
            if (_vignetteFading)
            {
                Debug.LogWarning("PostProcessController WARNING: Vignette is already fading.");
                return;
            }
            if (!CheckEffectEnabled<Vignette>())
                return;

            _vignetteCoroutine = BlinkVignette(color, intensity, smoothness, roundness, inDuration, waitDuration, outDuration, inCurve, outCurve);
            StartCoroutine(_vignetteCoroutine);
        }

        #endregion

        #endregion

        #region Instant overrides

        /// <summary>
        /// Instantly overrides the bloom effect if it is found on the target volume.
        /// </summary>
        /// <param name="intensity"> New intensity clamped between 0 and infinity. </param>
        /// <param name="threshold"> New threshold clamped between 0 and infinity. </param>
        /// <param name="softKnee"> New soft knee clamped between 0 and 1. </param>
        /// <param name="color"> New bloom color. </param>
        /// <param name="forceEnableLayer"> Forces the effect to enable if it's disabled. </param>
        public void OverrideBloomValues(float intensity, float threshold, float softKnee, Color color, bool forceEnableLayer = false)
        {
            if (!_volume.profile.TryGetSettings(out Bloom bloom))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a bloom effect.");
                return;
            }

            if (forceEnableLayer && !bloom.active || !bloom.enabled)
            {
                bloom.active = true;
                bloom.enabled.Override(true);
            }

            bloom.intensity.Override(intensity);
            bloom.threshold.Override(threshold);
            bloom.softKnee.Override(softKnee);
            bloom.color.Override(color);
        }

        /// <summary>
        /// Instantly overrides the chromatic aberration effect if it is found on the target volume.
        /// </summary>
        /// <param name="intensity"> New intensity clamped between 0 and 1. </param>
        /// <param name="fastMode"> New fast mode value. </param>
        /// <param name="forceEnableLayer"> Forces the effect to enable if it's disabled. </param>
        public void OverrideChromaticAberrationValues(float intensity, bool fastMode, bool forceEnableLayer = false)
        {
            if (!_volume.profile.TryGetSettings(out ChromaticAberration chromaticAberration))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a chromatic aberration effect.");
                return;
            }

            if (forceEnableLayer && !chromaticAberration.active || !chromaticAberration.enabled)
            {
                chromaticAberration.active = true;
                chromaticAberration.enabled.Override(true);
            }

            chromaticAberration.intensity.Override(intensity);
            chromaticAberration.fastMode.Override(fastMode);
        }

        /// <summary>
        /// Instantly overrides the depth of field effect if it is found on the target volume.
        /// </summary>
        /// <param name="focusDist"> New focus distance clamped between 0.1 and infinity. </param>
        /// <param name="aperture"> New aperture clamped between 0.1 and 32. </param>
        /// <param name="focalLength"> New focal length clamped between 1 and 300. </param>
        /// <param name="forceEnableLayer"> Forces the effect to enable if it's disabled. </param>
        public void OverrideDepthOfFieldValues(float focusDist, float aperture, float focalLength, bool forceEnableLayer = false)
        {
            if (!_volume.profile.TryGetSettings(out DepthOfField dof))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a depth of field effect.");
                return;
            }

            if (forceEnableLayer && !dof.active || !dof.enabled)
            {
                dof.active = true;
                dof.enabled.Override(true);
            }

            dof.focusDistance.Override(focusDist);
            dof.aperture.Override(aperture);
            dof.focalLength.Override(focalLength);
        }

        /// <summary>
        /// Instantly overrides the grain effect if it is found on the target volume.
        /// </summary>
        /// <param name="colored"> New colored value. </param>
        /// <param name="intensity"> New intensity clamped between 0 and 1. </param>
        /// <param name="size"> New size clamped between 0.3 and 3. </param>
        /// <param name="luminanceContribution"> New luminance contribution clamped between 0 and 1. Default is 0.8. </param>
        /// <param name="forceEnableLayer"> Forces the effect to enable if it's disabled. </param>
        public void OverrideGrainValues(bool colored, float intensity, float size, float luminanceContribution, bool forceEnableLayer = false)
        {
            if (!_volume.profile.TryGetSettings(out Grain grain))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a grain effect.");
                return;
            }

            if (forceEnableLayer && !grain.active || !grain.enabled)
            {
                grain.active = true;
                grain.enabled.Override(true);
            }

            grain.colored.Override(colored);
            grain.intensity.Override(intensity);
            grain.size.Override(size);
            grain.lumContrib.Override(luminanceContribution);
        }

        /// <summary>
        /// Instantly overrides the lens distorsion effect if it is found on the target volume.
        /// </summary>
        /// <param name="intensity"> New intensity clamped between -100 and 100. </param>
        /// <param name="xMultiplier"> New x intensity clamped between 0 and 1. </param>
        /// <param name="yMultiplier"> New y intensity clamped between 0 and 1. </param>
        /// <param name="centerX"> New x center clamped between -1 and 1. </param>
        /// <param name="centerY"> New y center clamped between -1 and 1. </param>
        /// <param name="scale"> New scale clamped between 0.01 and 5. </param>
        /// <param name="forceEnableLayer"> Forces the effect to enable if it's disabled. </param>
        public void OverrideLensDistorsionValues(float intensity, float xMultiplier, float yMultiplier, float centerX, float centerY, float scale, bool forceEnableLayer = false)
        {
            if (!_volume.profile.TryGetSettings(out LensDistortion lensDistorsion))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a lens distorsion effect.");
                return;
            }

            if (forceEnableLayer && !lensDistorsion.active || !lensDistorsion.enabled)
            {
                lensDistorsion.active = true;
                lensDistorsion.enabled.Override(true);
            }

            lensDistorsion.intensity.Override(intensity);
            lensDistorsion.intensityX.Override(xMultiplier);
            lensDistorsion.intensityY.Override(yMultiplier);
            lensDistorsion.centerX.Override(centerX);
            lensDistorsion.centerY.Override(centerY);
            lensDistorsion.scale.Override(scale);
        }

        /// <summary>
        /// Instantly overrides the vignette effect if it is found on the target volume.
        /// </summary>
        /// <param name="color"> New vignette color. </param>
        /// <param name="intensity"> New intensity clamped between 0 and 1. </param>
        /// <param name="smoothness"> New smoothness clamped between 0.01 and 1. </param>
        /// <param name="roundness"> New roundness clamped between 0 and 1. </param>
        /// <param name="rounded"> New rounded value. </param>
        /// <param name="forceEnableLayer"> Forces the effect to enable if it's disabled. </param>
        public void OverrideVignetteValues(Color color, float intensity, float smoothness, float roundness, bool rounded, bool forceEnableLayer = false)
        {
            if (!_volume.profile.TryGetSettings(out Vignette vignette))
            {
                Debug.LogWarning("PostProcessController WARNING: Volume doesn't have a vignette effect.");
                return;
            }

            if (forceEnableLayer && !vignette.active || !vignette.enabled)
            {
                vignette.active = true;
                vignette.enabled.Override(true);
            }

            vignette.color.Override(color);
            vignette.intensity.Override(intensity);
            vignette.smoothness.Override(smoothness);
            vignette.roundness.Override(roundness);
            vignette.rounded.Override(rounded);
        }

        #endregion
    }
}