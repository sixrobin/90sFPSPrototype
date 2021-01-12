namespace Doomlike
{
    using UnityEngine;

    public class TrainingWorkshop : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private TrainingTarget[] _targets = null;
        [SerializeField] private float _resetDelayAfterCompletion = 1f;
        [SerializeField] private float _resetDelayAfterNoShot = 5f;
        [SerializeField] private int _workshopIndex = 0;

        private bool _inProgress;
        private int _targetsShot;
        private float _timer;
        private float _noShotsTimer;
        private int _shots;

        public delegate void WorkshopCompleteEventHandler();

        public event WorkshopCompleteEventHandler WorkshopComplete;

        public float BestTime { get; private set; } = float.MaxValue;

        public int BestShots { get; private set; } = int.MaxValue;

        public string Score { get; private set; } = "D";

        public int Tries { get; private set; }

        public int WorkshopIndex => _workshopIndex;

        public string ConsoleProPrefix => "Training Workshop";

        [ContextMenu("Reset Score")]
        public void ResetScore()
        {
            ConsoleProLogger.Log(this, $"Resetting score for Training Workshop <b>{transform.name}</b>.", gameObject);

            Tries = 0;
            BestTime = float.MaxValue;
            BestShots = int.MaxValue;
            Score = "D";
        }

        private void OnTargetShot(TrainingTarget target)
        {
            if (_shots == 0)
                _shots++; // Listener method won't add a shot because the workshop is not in progress on first shot event.

            _targetsShot++;
            _inProgress = _targetsShot < _targets.Length;

            UnityEngine.Assertions.Assert.IsFalse(_targetsShot > _targets.Length, "Shot more targets than the training workshop actually has!");

            if (_targetsShot == _targets.Length)
            {
                ConsoleProLogger.Log(this, $"Training Workshop <b>{transform.name}</b> complete.", gameObject);

                WorkshopComplete?.Invoke();
                RecordScore();
                StartCoroutine(ResetWorkshopCoroutine());
            }
        }

        private void RecordScore()
        {
            ConsoleProLogger.Log(this, $"Recording score for Training Workshop <b>{transform.name}</b>.", gameObject);

            Tries++;
            BestTime = Mathf.Min(BestTime, _timer);
            BestShots = Mathf.Min(BestShots, _shots);
            // TODO: Compute some S/A/B/C/D score here.
        }

        private System.Collections.IEnumerator ResetWorkshopCoroutine()
        {
            yield return RSLib.Yield.SharedYields.WaitForSeconds(_resetDelayAfterCompletion);

            ConsoleProLogger.Log(this, $"Reseting Training Workshop <b>{transform.name}</b>.", gameObject);
            for (int i = _targets.Length - 1; i >= 0; --i)
                _targets[i].ResetTarget();

            _targetsShot = 0;
            _timer = 0f;
            _noShotsTimer = 0f;
            _shots = 0;
        }

        private void OnShot()
        {
            if (!_inProgress)
                return;

            _shots++;
            _noShotsTimer = 0f;
        }

        private void Awake()
        {
            Manager.ReferencesHub.FPSMaster.FPSShoot.Shot += OnShot;
            for (int i = _targets.Length - 1; i >= 0; --i)
                _targets[i].TargetShot += OnTargetShot;
        }

        private void Update()
        {
            // TODO: All this logic should be in a coroutine running while workshop is in progress.

            if (!_inProgress)
                return;

            _timer += Time.deltaTime;
            _noShotsTimer += Time.deltaTime;

            if (_noShotsTimer > _resetDelayAfterNoShot)
            {
                ConsoleProLogger.Log(this, $"Resetting Training Workshop <b>{transform.name}</b> while in progress.");

                _noShotsTimer = 0f;
                _inProgress = false;
                StartCoroutine(ResetWorkshopCoroutine());
            }
        }

        private void OnDestroy()
        {
            if (Manager.ReferencesHub.Exists())
                Manager.ReferencesHub.FPSMaster.FPSShoot.Shot -= OnShot;

            for (int i = _targets.Length - 1; i >= 0; --i)
                _targets[i].TargetShot -= OnTargetShot;
        }
    }
}