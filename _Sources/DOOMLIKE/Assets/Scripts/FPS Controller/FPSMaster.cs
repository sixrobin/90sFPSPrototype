using UnityEngine;

/// <summary>
/// Main class for the FPS controller. Controls the more generic things of the controller.
/// </summary>
public class FPSMaster : MonoBehaviour
{
    [SerializeField] private FPSControllableComponent[] _allComponents = null;
    [SerializeField] private FPSCameraShake _fpsCameraShake = null;
    [SerializeField] private OptionsManager _optionsManager = null;

    public FPSCameraShake FPSCameraShake => _fpsCameraShake;

    public OptionsManager OptionsManager => _optionsManager;

    /// <summary>
    /// Enables all the controllable components of the FPS controller.
    /// </summary>
    [ContextMenu("Enable All Components")]
    public void EnableAllComponents()
    {
        foreach (FPSControllableComponent component in _allComponents)
            component.Controllable = true;
    }

    /// <summary>
    /// Disables all the controllable components of the FPS controller.
    /// </summary>
    [ContextMenu("Disable All Components")]
    public void DisableAllComponents()
    {
        foreach (FPSControllableComponent component in _allComponents)
            component.Controllable = false;
    }

    private void Awake()
    {
        foreach (FPSControllableComponent component in _allComponents)
            component.SetFPSMaster(this);
    }
}