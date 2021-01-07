using UnityEngine;

public class TrainingWorkshopTerminal : FPSInteraction
{
    [SerializeField] private TrainingWorkshop _trainingWorkshop = null;
    [SerializeField] private MeshRenderer _terminalRenderer = null;
    [SerializeField] private Material _terminalOnMaterial = null;
    [SerializeField] private cakeslice.Outline _outline = null;

    private Material _terminalOffMaterial = null;
    private bool _isOn = false;

    public override void Focus()
    {
        base.Focus();
        _outline.eraseRenderer = false;
    }

    public override void Unfocus()
    {
        base.Unfocus();
        _outline.eraseRenderer = true;
    }

    public override void Interact()
    {
        base.Interact();

        _isOn = !_isOn;
        _terminalRenderer.material = _isOn ? _terminalOnMaterial : _terminalOffMaterial;

        Debug.Log("Opening/Shutting down training workshop terminal...", gameObject);
        Debug.Log($"Best time: {(_trainingWorkshop.BestTime == float.MaxValue ? 0f : _trainingWorkshop.BestTime)} seconds.", gameObject);
        Debug.Log($"Best shots: {(_trainingWorkshop.BestShots == int.MaxValue ? 0 : _trainingWorkshop.BestShots)} shots.", gameObject);
    }

    protected override void Awake()
    {
        base.Awake();
        _terminalOffMaterial = _terminalRenderer.material;
        _outline.eraseRenderer = true;
    }
}