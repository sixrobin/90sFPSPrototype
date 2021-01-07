using UnityEngine;

/// <summary>
/// Main class for the FPS controller. Controls the more generic things of the controller.
/// </summary>
public class FPSMaster : MonoBehaviour
{
    [SerializeField] private FPSControllableComponent[] _allComponents = null;
    [SerializeField] private FPSController _fpsController = null;
    [SerializeField] private FPSCameraShake _fpsCameraShake = null;
    [SerializeField] private OptionsManager _optionsManager = null;

    public FPSController FPSController => _fpsController;

    public FPSCameraShake FPSCameraShake => _fpsCameraShake;

    public OptionsManager OptionsManager => _optionsManager;

    /// <summary>
    /// Enables all the controllable components of the FPS controller.
    /// </summary>
    [ContextMenu("Enable All Components")]
    public void EnableAllComponents()
    {
        for (int i = _allComponents.Length - 1; i >= 0; --i)
            _allComponents[i].Controllable = true;
    }

    /// <summary>
    /// Disables all the controllable components of the FPS controller.
    /// </summary>
    [ContextMenu("Disable All Components")]
    public void DisableAllComponents()
    {
        for (int i = _allComponents.Length - 1; i >= 0; --i)
            _allComponents[i].Controllable = false;
    }

    private void Awake()
    {
        foreach (FPSControllableComponent component in _allComponents)
            component.SetFPSMaster(this);
    }
}