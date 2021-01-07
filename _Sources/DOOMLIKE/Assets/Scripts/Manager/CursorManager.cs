using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private OptionsManager _optionsManager = null;

    public void Awake()
    {
        _optionsManager.OptionsStateChanged += OnOptionsStateChanged;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnOptionsStateChanged(bool state)
    {
        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Confined;
    }
}