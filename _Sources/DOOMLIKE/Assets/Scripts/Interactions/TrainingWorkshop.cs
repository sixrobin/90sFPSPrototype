using UnityEngine;

public class TrainingWorkshop : MonoBehaviour
{
    [SerializeField] private TrainingTarget[] _targets = null;
    [SerializeField] private float _resetDelay = 1f;

    private int _targetsShot = 0;

    public delegate void WorkshopCompleteEventHandler();
    public event WorkshopCompleteEventHandler WorkshopComplete;

    private void OnTargetShot(TrainingTarget target)
    {
        _targetsShot++;

        UnityEngine.Assertions.Assert.IsFalse(_targetsShot > _targets.Length, "Shot more targets than the training workshop actually has!");

        if (_targetsShot == _targets.Length)
        {
            WorkshopComplete?.Invoke();
            StartCoroutine(ResetTargetsCoroutine());
        }
    }

    private System.Collections.IEnumerator ResetTargetsCoroutine()
    {
        yield return RSLib.Yield.SharedYields.WaitForSeconds(_resetDelay);

        Debug.Log("Reseting training workshop.", gameObject);
        for (int i = _targets.Length - 1; i >= 0; --i)
            _targets[i].ResetTarget();

        _targetsShot = 0;
    }

    private void Awake()
    {
        for (int i = _targets.Length - 1; i >= 0; --i)
            _targets[i].TargetShot += OnTargetShot;
    }

    private void OnDestroy()
    {
        for (int i = _targets.Length - 1; i >= 0; --i)
            _targets[i].TargetShot -= OnTargetShot;
    }
}