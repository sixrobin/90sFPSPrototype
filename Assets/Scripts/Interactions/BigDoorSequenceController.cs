namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    public class BigDoorSequenceController : MonoBehaviour
    {
        [SerializeField] private Transform _door = null;
        [SerializeField] private float _openDuration = 3f;
        [SerializeField] private float _targetDoorHeight = 2f;
        [SerializeField, Range(0f, 1f)] private float _lookAtSpeed = 1.5f;
        [SerializeField] private RSLib.EasingCurves.Curve _openCurve = RSLib.EasingCurves.Curve.OutBounce;
        [SerializeField, Range(0f, 1f)] private float _constantTrauma = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _endTrauma = 0.45f;

        public void TriggerOpenDoorSequence()
        {
            StartCoroutine(OpenDoorCoroutine());
        }

        private System.Collections.IEnumerator OpenDoorCoroutine()
        {
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;

            Manager.ReferencesHub.FPSMaster.DisableAllComponentsAtEndOfFrame();
            Manager.ReferencesHub.FPSMaster.FPSUIController.Hide();

            yield return RSLib.Yield.SharedYields.WaitForSeconds(0.3f);

            yield return StartCoroutine(Manager.ReferencesHub.FPSMaster.FPSCamera.LookAt(_door.position.AddY(_targetDoorHeight * 0.25f), _lookAtSpeed));

            yield return RSLib.Yield.SharedYields.WaitForSeconds(1f);

            Vector3 doorInitPos = _door.position;
            Vector3 doorTargetPos = doorInitPos.AddY(_targetDoorHeight);

            for (float t = 0f; t < 1f; t += Time.deltaTime / _openDuration)
            {
                Manager.ReferencesHub.FPSMaster.FPSCameraShake.SetTrauma(_constantTrauma);
                _door.position = Vector3.Lerp(doorInitPos, doorTargetPos, RSLib.EasingCurves.Easing.Ease(t, _openCurve));
                yield return null;
            }

            _door.position = doorTargetPos;
            Manager.ReferencesHub.FPSMaster.FPSCameraShake.SetTrauma(_endTrauma);

            yield return RSLib.Yield.SharedYields.WaitForSeconds(1.5f);

            Manager.ReferencesHub.FPSMaster.EnableAllComponents();
            Manager.ReferencesHub.FPSMaster.FPSUIController.Show();

            //Manager.ReferencesHub.FPSMaster.FPSCamera.Recenter(180f);
        }

        private void OnDrawGizmosSelected()
        {
            if (_door == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_door.position, _door.position.AddY(_targetDoorHeight));
            Gizmos.DrawWireSphere(_door.position.AddY(_targetDoorHeight), 0.2f);
        }
    }
}