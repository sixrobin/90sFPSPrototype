using UnityEngine;

public abstract class OptionRaycasterValues : OptionRaycaster
{
    [SerializeField] protected TMPro.TextMeshProUGUI _valueText = null;
    [SerializeField] private string[] _values = null;

    private int _currentValueIndex = 0;

    public override void OnClicked()
    {
        _currentValueIndex++;
        if (_currentValueIndex == _values.Length)
            _currentValueIndex = 0;

        _valueText.text = FormatValueDisplay(_values[_currentValueIndex]);
        OnValueChanged(_values[_currentValueIndex]);
    }

    public virtual string FormatValueDisplay(string display)
    {
        return display;
    }

    public abstract void OnValueChanged(string value);

    protected override void Start()
    {
        base.Start();
        _currentValueIndex = System.Array.IndexOf(_values, _valueText.text);
        _valueText.text = FormatValueDisplay(_values[_currentValueIndex]);
    }
}