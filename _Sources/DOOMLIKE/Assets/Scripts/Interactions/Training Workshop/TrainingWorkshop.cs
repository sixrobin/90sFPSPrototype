namespace Doomlike
{
    using UnityEngine;

    public class TrainingWorkshop : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private TrainingTarget[] _targets = null;
        [SerializeField] private float _resetDelayAfterCompletion = 1f;
        [SerializeField] private float _resetDelayAfterNoShot = 5f;
        [SerializeField] private int _workshopIndex = 0;

        [Header("SCORE")]
        [SerializeField, Min(0f)] private float _timeFirstThreshold = 3f;
        [SerializeField, Min(0f)] private float _timeSecondThreshold = 1f;
        [SerializeField] private int _shotsFirstThreshold = 0;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;

        private bool _inProgress;
        private int _targetsShot;
        private float _timer;
        private float _noShotsTimer;
        private int _shots;
        private int _score;

        private string[] _scoresString = new string[] { "S", "A", "B", "C", "D" }; // 01234 = SABCD.

        public delegate void WorkshopCompleteEventHandler();

        public event WorkshopCompleteEventHandler WorkshopComplete;

        public float BestTime { get; private set; } = float.MaxValue;

        public int BestShots { get; private set; } = int.MaxValue;

        public int BestScore { get; private set; } = 4;

        public int Tries { get; private set; }

        public int WorkshopIndex => _workshopIndex;

        public string ConsoleProPrefix => "Training Workshop";

        public bool ConsoleProMuted => _logsMuted;

        [ContextMenu("Reset Score")]
        public void ResetScore()
        {
            ConsoleProLogger.Log(this, $"Resetting score for Training Workshop <b>{transform.name}</b>.", gameObject);

            Tries = 0;
            BestTime = float.MaxValue;
            BestShots = int.MaxValue;
            BestScore = 4;
        }

        public string GetBestScoreToString()
        {
            UnityEngine.Assertions.Assert.IsTrue(BestScore >= 0 && BestScore < _scoresString.Length, $"Score value {BestScore} is out of score strings array bounds (length of {_scoresString.Length}).");
            return _scoresString[BestScore];
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
            UnityEngine.Assertions.Assert.IsTrue(_shotsFirstThreshold > _targets.Length,
                $"Shots first threshold for Training Workshop <b>{transform.name}</b> ({_shotsFirstThreshold} must be higher than targets count which is the second threshold ({_targets.Length}).");

            UnityEngine.Assertions.Assert.IsFalse(_shots < _targets.Length,
                $"Workshop has been completed in less shots ({_shots}) than there are targets ({_targets.Length}).");


            ConsoleProLogger.Log(this, $"Recording score for Training Workshop <b>{transform.name}</b>.", gameObject);

            Tries++;

            _score = 4;
            if (_timer < _timeFirstThreshold) _score--;
            if (_timer < _timeSecondThreshold) _score--;
            if (_shots < _shotsFirstThreshold) _score--;
            if (_shots == _targets.Length) _score--;

            BestTime = Mathf.Min(BestTime, _timer);
            BestShots = Mathf.Min(BestShots, _shots);
            BestScore = Mathf.Min(BestScore, _score);
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
            // This requires the ResetWorkshopCoroutine to be stored in a variable to handle them a better and safer way.

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