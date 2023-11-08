namespace RSLib
{
    using RSLib.EasingCurves;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    public class PostProcessBlender : MonoBehaviour
    {
        public static PostProcessBlender Instance;
        private IEnumerator _crossFadeCoroutine;
        private bool _crossFading;

        public bool TimeScaleDependent = true;

        private IEnumerator CrossFade(PostProcessVolume current, PostProcessVolume next, float targetWeight, float duration, Curve outCurve, Curve inCurve)
        {
            _crossFading = true;
            float initWeight = current.weight;

            for (float t = 0; t < 1; t += (TimeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime) / duration)
            {
                current.weight = Mathf.Lerp(initWeight, 0, Easing.Ease(t, outCurve));
                next.weight = Mathf.Lerp(0, targetWeight, Easing.Ease(t, inCurve));
                yield return null;
            }

            current.weight = 0;
            next.weight = targetWeight;
            _crossFading = false;
        }

        public void CrossFadeVolumes(PostProcessVolume current, PostProcessVolume next, float targetWeight, float duration, Curve outCurve, Curve inCurve)
        {
            if (_crossFading)
            {
                Debug.LogWarning("PostProcessBlender WARNING: 2 volumes are already crossfading.");
                return;
            }
            if (current.weight <= next.weight)
                Debug.LogWarning("PostProcessBlender WARNING: Target volume's weight is greater than source volume.");

            _crossFadeCoroutine = CrossFade(current, next, targetWeight, duration, outCurve, inCurve);
            StartCoroutine(_crossFadeCoroutine);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}