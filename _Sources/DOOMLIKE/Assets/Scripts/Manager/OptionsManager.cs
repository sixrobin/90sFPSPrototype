using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private TimeManager _timeManager = null;
    [SerializeField] private GameObject _optionsView = null;

    public delegate void OptionsStateChangedEventHandler(bool state);

    public event OptionsStateChangedEventHandler OptionsStateChanged;

    public bool IsOpen { get; private set; } = false;

    private void OpenClose()
    {
        IsOpen = !IsOpen;
        _optionsView.SetActive(IsOpen);

        if (IsOpen)
            _timeManager.Freeze();
        else
            _timeManager.Unfreeze();

        OptionsStateChanged(IsOpen);
    }

    private void Start()
    {
        _optionsView.SetActive(IsOpen);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OpenClose();
    }
}