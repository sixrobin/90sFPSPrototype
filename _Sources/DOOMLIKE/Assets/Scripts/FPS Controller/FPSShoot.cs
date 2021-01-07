using RSLib.Extensions;
using UnityEngine;

public class FPSShoot : FPSControllableComponent
{
    [Header("REFERENCES")]
    [SerializeField] private Transform _cameraTransform = null;
    [SerializeField] private Animator _weaponAnimator = null;
    [SerializeField] private FPSWeaponView _weaponView = null;
    
    [Header("IMPACT")]
    [SerializeField] private GameObject[] _bulletImpactPrefabs = null;
    [SerializeField] private float _shootTrauma = 0.15f;

    [Header("DEBUG")]
    [SerializeField] private bool _dbg = true;

    private bool _canShoot = true;
    private bool _shooting = false; // Animating running.

    private System.Collections.Generic.Dictionary<Collider, IFPSShootable> _knownShootables = new System.Collections.Generic.Dictionary<Collider, IFPSShootable>();

    public delegate void ShotEventHandler();
    public event ShotEventHandler OnShot;

    protected override void OnControlAllowed()
    {
        base.OnControlAllowed();
        _weaponView.Display(true);
    }

    protected override void OnControlDisallowed()
    {
        base.OnControlDisallowed();
        _weaponView.Display(false);
    }

    private void TryShoot()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (_dbg)
                Debug.Log("Triggering Shoot animation!");

            _shooting = true;
            _weaponAnimator.SetTrigger("Shoot");
        }
    }

    private void ApplyShoot()
    {
        if (_dbg)
            Debug.Log("Shooting!");

        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.Log($"Shooting on {hit.transform.name}", gameObject);

            if (!_knownShootables.TryGetValue(hit.collider, out IFPSShootable shootable))
                if (hit.collider.TryGetComponent(out shootable))
                    _knownShootables.Add(hit.collider, shootable);

            if (shootable != null)
            {
                shootable.OnShot(hit.point);
                FPSMaster.FPSCameraShake.SetTrauma(shootable.TraumaOnShot);
            }

            // Bullet impacts.
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                Transform bulletImpactInstance = Instantiate(_bulletImpactPrefabs.Any(), hit.transform).transform;
                bulletImpactInstance.position = hit.point + hit.normal * 0.01f;
                bulletImpactInstance.forward = -hit.normal;
                bulletImpactInstance.Rotate(0f, 0f, Random.Range(0, 4) * 90);
            }
        }

        OnShot?.Invoke();
        FPSMaster.FPSCameraShake.AddTrauma(_shootTrauma);
    }

    private void UpdateAnimator()
    {
        _weaponAnimator.SetBool("Moving", FPSMaster.FPSController.IsMoving);
        _weaponAnimator.SetBool("Sprinting", FPSMaster.FPSController.Sprinting);
        _weaponAnimator.SetBool("Crouched", FPSMaster.FPSController.Crouched);
    }

    private void OnOptionsStateChanged(bool state)
    {
        _canShoot = !state;
    }

    private void OnShootAnimationOver()
    {
        _shooting = false;
    }

    private void OnShootFrame()
    {
        ApplyShoot();
    }

    private void Start()
    {
        FPSMaster.OptionsManager.OptionsStateChanged += OnOptionsStateChanged;
        _weaponView.ShootAnimationOver += OnShootAnimationOver;
        _weaponView.ShootFrame += OnShootFrame;
    }

    private void Update()
    {
        if (!Controllable || !_canShoot || _shooting)
            return;

        TryShoot();
        UpdateAnimator();
    }

    private void OnDestroy()
    {
        FPSMaster.OptionsManager.OptionsStateChanged -= OnOptionsStateChanged;
        _weaponView.ShootAnimationOver -= OnShootAnimationOver;
        _weaponView.ShootFrame -= OnShootFrame;
    }
}