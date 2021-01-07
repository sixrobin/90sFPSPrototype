using UnityEngine;

public class OptionPixelPerfect : OptionRaycasterToggle
{
    [SerializeField] private UnityEngine.U2D.PixelPerfectCamera[] _pixelPerfectCameras = null;

    public override void Init()
    {
        OnToggleValueChanged(_toggle.isOn);
    }

    public override void OnToggleValueChanged(bool value)
    {
        for (int i = _pixelPerfectCameras.Length - 1; i >= 0; --i)
            _pixelPerfectCameras[i].enabled = value;

        Debug.Log($"{(value ? "Enabled" : "Disabled")} {_pixelPerfectCameras.Length} pixel perfect camera components.");
    }

#if UNITY_EDITOR
    [ContextMenu("Find all pixel perfect cameras")]
    private void FindAllPixelPerfectCameras()
    {
        _pixelPerfectCameras = FindObjectsOfType<UnityEngine.U2D.PixelPerfectCamera>();
    }
#endif
}