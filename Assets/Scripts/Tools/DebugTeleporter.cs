namespace Doomlike.Tools
{
    using UnityEngine;

    public class DebugTeleporter : MonoBehaviour
    {
        [SerializeField] private Transform[] _tpPoints = null;
        [SerializeField] private Color _pointsGizmosColor = Color.yellow;
        [SerializeField, Min(0f)] private float _pointsGizmosRadius = 0.3f;

        [ContextMenu("Get Children as Points")]
        private void GetChildrenAsPoints()
        {
            _tpPoints = new Transform[transform.childCount];
            for (int i = 0; i < _tpPoints.Length; ++i)
                _tpPoints[i] = transform.GetChild(i);
        }

        private void TeleportToPoint(int index)
        {
            if (_tpPoints == null || _tpPoints.Length == 0)
            {
                Console.DebugConsole.LogExternalError("No teleport point has been set.");
                return;
            }

            if (index < 0 || index >= _tpPoints.Length)
            {
                Console.DebugConsole.LogExternalError($"Specified index {index} is out of teleport points array bounds.");
                return;
            }

            Vector3 pos = _tpPoints[index].position;
            Manager.ReferencesHub.FPSMaster.FPSController.transform.position = pos;
            Console.DebugConsole.LogExternal($"Teleporting player to point {index} (X:{pos.x} / Y:{pos.y} / Z:{pos.z}).");
        }

        private void Awake()
        {
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand<int>(
                "tpAtIndex",
                "Teleports the player to debug position at specified index.",
                true,
                false,
                (index) => TeleportToPoint(index)));
        }

        private void OnDrawGizmosSelected()
        {
            if (_tpPoints == null || _tpPoints.Length == 0)
                return;

            Gizmos.color = _pointsGizmosColor;
            for (int i = _tpPoints.Length - 1; i >= 0; --i)
                Gizmos.DrawWireSphere(_tpPoints[i].position, _pointsGizmosRadius);
        }
    }
}