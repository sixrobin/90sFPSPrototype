namespace Doomlike.FPSCtrl
{
    using RSLib.Extensions;
    using UnityEngine;

    public class FPSShoot : FPSControllableComponent, IConsoleProLoggable
    {
        [Header("REFERENCES")]
        [SerializeField] private Transform _camTransform = null;
        [SerializeField] private Animator _weaponAnimator = null;
        [SerializeField] private FPSWeaponView _weaponView = null;

        [Header("MAGAZINE")]
        [SerializeField] private FPSMagazine _initMagazine = new FPSMagazine(300, 30);

        [Header("IMPACT")]
        [SerializeField] private GameObject[] _bulletImpactPrefabs = null;
        [SerializeField, Range(0f, 1f)] private float _shootTrauma = 0.15f;

        [Header("INPUTS DELAY")]
        [SerializeField, Min(0f)] private float _shootInputDelay = 0.15f;
        [SerializeField, Min(0f)] private float _reloadInputDelay = 0.15f;

        private bool _canShoot = true;

        private bool _shootInput;
        private bool _reloadInput;
        private System.Collections.IEnumerator _shootInputDelayCoroutine;
        private System.Collections.IEnumerator _reloadInputDelayCoroutine;

        private bool _isShooting; // Animating running.
        private bool _isReloading; // Animating running.

        private System.Collections.Generic.Dictionary<Collider, IFPSShootable> _knownShootables = new System.Collections.Generic.Dictionary<Collider, IFPSShootable>();

        public delegate void ShotEventHandler();
        public delegate void MagazineChangedEventHandler(FPSMagazine magazine);

        public event ShotEventHandler Shot;
        public event MagazineChangedEventHandler MagazineChanged;

        private FPSMagazine _fpsMagazine;
        public FPSMagazine FPSMagazine
        {
            get => _fpsMagazine;
            set
            {
                _fpsMagazine = value;
                MagazineChanged(_fpsMagazine);
            }
        }

        public string ConsoleProPrefix => "FPS Shoot";

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

        private void TryReload()
        {
            if (!_reloadInput && Input.GetButtonDown("Reload"))
            {
                ResetReloadInputDelay();
                ResetShootInputDelay();

                _reloadInput = true;
                _reloadInputDelayCoroutine = DelayReloadInput();
                StartCoroutine(_reloadInputDelayCoroutine);
            }

            if (_reloadInput && FPSMagazine.CanReload && !_isShooting && !_isReloading)
                TriggerReload();
        }

        private void TryShoot()
        {
            if (!_shootInput && Input.GetButtonDown("Fire1"))
            {
                ResetShootInputDelay();
                ResetReloadInputDelay();

                _shootInput = true;
                _shootInputDelayCoroutine = DelayShootInput();
                StartCoroutine(_shootInputDelayCoroutine);
            }

            if (_shootInput && !_isShooting && !_isReloading)
            {
                if (FPSMagazine.IsLoadEmpty && !FPSMaster.DbgGodMode)
                {
                    if (FPSMagazine.IsCompletelyEmpty)
                    {
                        ConsoleProLogger.Log(this, "Trying to shoot with a completely empty magazine.", gameObject);
                        return;
                    }

                    ConsoleProLogger.Log(this, "Trying to shoot with an empty magazine load, reloading instead.", gameObject);
                    TriggerReload();
                    ResetShootInputDelay();
                }
                else
                {
                    ConsoleProLogger.Log(this, "Triggering shoot animation.", gameObject);
                    _isShooting = true;
                    _weaponAnimator.SetTrigger("Shoot");
                    ResetShootInputDelay();
                }
            }
        }

        private System.Collections.IEnumerator DelayShootInput()
        {
            yield return RSLib.Yield.SharedYields.WaitForSeconds(_shootInputDelay);
            _shootInput = false;
        }

        private void ResetShootInputDelay()
        {
            if (_shootInputDelayCoroutine != null)
                StopCoroutine(_shootInputDelayCoroutine);

            _shootInputDelayCoroutine = null;
            _shootInput = false;
        }

        private System.Collections.IEnumerator DelayReloadInput()
        {
            yield return RSLib.Yield.SharedYields.WaitForSeconds(_reloadInputDelay);
            _reloadInput = false;
        }

        private void ResetReloadInputDelay()
        {
            if (_reloadInputDelayCoroutine != null)
                StopCoroutine(_reloadInputDelayCoroutine);

            _reloadInputDelayCoroutine = null;
            _reloadInput = false;
        }

        private void TriggerReload()
        {
            ConsoleProLogger.Log(this, "Triggering reload animation.", gameObject);
            _isReloading = true;
            _weaponAnimator.SetTrigger("Reload");
        }

        private void ApplyReload()
        {
            ConsoleProLogger.Log(this, "Applying reload.", gameObject);
            FPSMagazine.Reload();
        }

        private void ApplyShot()
        {
            ConsoleProLogger.Log(this, "Applying shot.", gameObject);

            if (!FPSMaster.DbgGodMode)
            {
                UnityEngine.Assertions.Assert.IsFalse(FPSMagazine.IsLoadEmpty, "Shoot animation has been allowed with an empty magazine load.");
                FPSMagazine.Shoot();
            }

            ConsoleProLogger.Log(this, $"Magazine state : ({FPSMagazine.CurrLoadCount}/{FPSMagazine.CurrStorehouseCount}).", gameObject);

            if (Physics.Raycast(_camTransform.position, _camTransform.forward, out RaycastHit hit, Mathf.Infinity))
            {
                ConsoleProLogger.Log(this, $"Shot <b>{hit.transform.name}</b>.", gameObject);

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
                    ConsoleProLogger.Log(this, $"Instantiating bullet impact.", gameObject);

                    Transform bulletImpactInstance = Instantiate(_bulletImpactPrefabs.Any(), hit.transform).transform;
                    bulletImpactInstance.position = hit.point + hit.normal * 0.01f;
                    bulletImpactInstance.forward = -hit.normal;
                    bulletImpactInstance.Rotate(0f, 0f, Random.Range(0, 4) * 90);

                    Vector3 scale = bulletImpactInstance.localScale;
                    scale.Scale(new Vector3(1f / hit.transform.localScale.x, 1f / hit.transform.localScale.y, 1f / hit.transform.localScale.z));
                    bulletImpactInstance.localScale = scale;
                }
            }

            Shot?.Invoke();
            FPSMaster.FPSCameraShake.AddTrauma(_shootTrauma);
        }

        private void UpdateAnimator()
        {
            _weaponAnimator.SetBool("Moving", FPSMaster.FPSController.CheckMovement());
            _weaponAnimator.SetBool("Sprinting", FPSMaster.FPSController.Sprinting);
            _weaponAnimator.SetBool("Crouched", FPSMaster.FPSController.Crouched);
        }

        private void SetMagazine(int fullCapacity, int loadCapacity)
        {
            FPSMagazine = new FPSMagazine(fullCapacity, loadCapacity);
        }

        private void SetMagazine(FPSMagazine magazine)
        {
            FPSMagazine = new FPSMagazine(magazine);
        }

        private void OnOptionsStateChanged(bool state)
        {
            _canShoot = !state;
        }

        private void OnShootAnimationOver()
        {
            _isShooting = false;
        }

        private void OnReloadAnimationOver()
        {
            _isReloading = false;
        }

        private void OnShootFrame()
        {
            ApplyShot();
        }

        private void OnReloadFrame()
        {
            ApplyReload();
        }

        private void Start()
        {
            Manager.ReferencesHub.OptionsManager.OptionsStateChanged += OnOptionsStateChanged;
            _weaponView.ShootAnimationOver += OnShootAnimationOver;
            _weaponView.ReloadAnimationOver += OnReloadAnimationOver;
            _weaponView.ShootFrame += OnShootFrame;
            _weaponView.ReloadFrame += OnReloadFrame;

            SetMagazine(_initMagazine);

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand<int, int>("setMagazine", "Sets the weapon a new magazine.", SetMagazine));
        }

        private void Update()
        {
            if (!Controllable || !_canShoot)
                return;

            TryReload();
            TryShoot();
            UpdateAnimator();
        }

        private void OnDestroy()
        {
            if (Manager.ReferencesHub.Exists())
                Manager.ReferencesHub.OptionsManager.OptionsStateChanged -= OnOptionsStateChanged;

            _weaponView.ShootAnimationOver -= OnShootAnimationOver;
            _weaponView.ShootFrame -= OnShootFrame;
        }
    }
}