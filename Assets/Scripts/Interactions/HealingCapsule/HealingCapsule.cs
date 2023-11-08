namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    public class HealingCapsule : OutlinedInteraction
    {
        [Header("-1 FOR FULL HEAL")]
        [SerializeField, Min(0f)] private int _healAmount = 50;

        [Header("USE ANIMATION")]
        [SerializeField] private Transform _healScaler = null;
        [SerializeField, Min(0f)] private float _scaleDownDur = 1f;
        [SerializeField] private RSLib.EasingCurves.Curve _scaleDownCurve = RSLib.EasingCurves.Curve.InOutSine;

        public override void Interact()
        {
            base.Interact();

            if (_healAmount == -1)
                Manager.ReferencesHub.FPSMaster.FPSHealthSystem.HealFull();
            else
                Manager.ReferencesHub.FPSMaster.FPSHealthSystem.Heal(_healAmount);

            StartCoroutine(ScaleDownCoroutine());
        }

        [ContextMenu("Restore Capsule")]
        private void RestoreCapsule()
        {
            gameObject.SetActive(true);
            _healScaler.SetScaleY(1f);
            SetInteractionAvailability(true);
        }

        private System.Collections.IEnumerator ScaleDownCoroutine()
        {
            for (float t = 0; t < 1f; t += Time.deltaTime / _scaleDownDur)
            {
                _healScaler.SetScaleY(1f - RSLib.EasingCurves.Easing.Ease(t, _scaleDownCurve));
                yield return null;
            }

            gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            if (!Manager.DebugManager.DebugViewOn)
                return;

            Camera mainCamera = Camera.main;
            if ((mainCamera.transform.position - transform.position).sqrMagnitude > 4f)
                return;

            Vector3 worldPos = mainCamera.WorldToScreenPoint(transform.position);
            if (worldPos.z < 0f)
                return;

            worldPos.y = Screen.height - worldPos.y;
            GUI.Label(new Rect(worldPos.x, worldPos.y, 200f, 100f), $"Heal: {(_healAmount == -1 ? "Full" : _healAmount.ToString())}", Manager.DebugManager.WorldSpaceDebugStyle);
        }
    }
}