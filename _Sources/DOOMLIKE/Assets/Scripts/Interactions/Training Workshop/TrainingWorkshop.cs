using UnityEngine;

public class TrainingWorkshop : MonoBehaviour
{
    [SerializeField] private TrainingTarget[] _targets = null;
    [SerializeField] private float _resetDelayAfterCompletion = 1f;
    [SerializeField] private float _resetDelayAfterNoShot = 5f;

    [SerializeField] private FPSShoot _fpsShoot = null;

    private int _targetsShot = 0;
    private bool _inProgress = false;
    private float _timer = 0f;
    private float _noShotsTimer = 0f;
    private int _shots = 0;

    public float BestTime { get; private set; } = float.MaxValue;

    public int BestShots { get; private set; } = int.MaxValue;

    public delegate void WorkshopCompleteEventHandler();
    public event WorkshopCompleteEventHandler WorkshopComplete;

    private void OnTargetShot(TrainingTarget target)
    {
        if (_shots == 0)
            _shots++; // Listener method won't add a shot because the workshop is not in progress on first shot event.

        _targetsShot++;
        _inProgress = _targetsShot < _targets.Length;

        UnityEngine.Assertions.Assert.IsFalse(_targetsShot > _targets.Length, "Shot more targets than the training workshop actually has!");

        if (_targetsShot == _targets.Length)
        {
            Debug.Log("Workshop complete!", gameObject);

            WorkshopComplete?.Invoke();
            RecordScore();
            StartCoroutine(ResetWorkshopCoroutine());
        }
    }

    private void RecordScore()
    {
        BestTime = Mathf.Min(BestTime, _timer);
        BestShots = Mathf.Min(BestShots, _shots);
        // TODO: Compute some S/A/B/C/D score here.
    }

    [ContextMenu("Reset Score")]
    public void ResetScore()
    {
        BestTime = float.MaxValue;
        BestShots = int.MaxValue;
    }

    private System.Collections.IEnumerator ResetWorkshopCoroutine()
    {
        yield return RSLib.Yield.SharedYields.WaitForSeconds(_resetDelayAfterCompletion);

        Debug.Log("Reseting training workshop.", gameObject);
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
        _fpsShoot.OnShot += OnShot;
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
            Debug.Log("Resetting workshop while in progress...");
            
            _noShotsTimer = 0f;
            _inProgress = false;
            StartCoroutine(ResetWorkshopCoroutine());
        }
    }

    private void OnDestroy()
    {
        _fpsShoot.OnShot -= OnShot;
        for (int i = _targets.Length - 1; i >= 0; --i)
            _targets[i].TargetShot -= OnTargetShot;
    }
}