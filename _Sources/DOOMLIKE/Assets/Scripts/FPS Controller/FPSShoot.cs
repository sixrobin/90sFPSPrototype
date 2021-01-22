namespace Doomlike.FPSCtrl
{
    using RSLib.Extensions;
    using UnityEngine;

    public class FPSShoot : FPSControllableComponent, IConsoleProLoggable
    {
        private const string INPUT_SHOOT = "Fire1";
        private const string INPUT_RELOAD = "Reload";

        private const string ANM_PARAM_MOVING = "Moving";
        private const string ANM_PARAM_SPRINTING = "Sprinting";
        private const string ANM_PARAM_CROUCHED = "Crouched";
        private const string ANM_PARAM_SHOOT = "Shoot";
        private const string ANM_PARAM_RELOAD = "Reload";

        [Header("REFERENCES")]
        [SerializeField] private Transform _camTransform = null;
        [SerializeField] private Animator _weaponAnimator = null;
        [SerializeField] private FPSWeaponView _weaponView = null;

        [Header("MASK")]
        [SerializeField] private LayerMask _shootableMask = 0;

        [Header("MAGAZINE")]
        [SerializeField] private FPSMagazine _initMagazine = new FPSMagazine(300, 30);

        [Header("IMPACT")]
        [SerializeField] private GameObject[] _bulletImpactPrefabs = null;
        [SerializeField, Range(0f, 1f)] private float _shootTrauma = 0.15f;

        [Header("INPUTS DELAY")]
        [SerializeField, Min(0f)] private float _shootInputDelay = 0.15f;
        [SerializeField, Min(0f)] private float _reloadInputDelay = 0.15f;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;

        private bool _canShoot = true;

        private bool _shootInput;
        private bool _reloadInput;
        private System.Collections.IEnumerator _shootInputDelayCoroutine;
        private System.Collections.IEnumerator _reloadInputDelayCoroutine;

        private bool _isShooting; // Animating running.
        private bool _isReloading; // Animating running.

        private System.Collections.Generic.Dictionary<Collider, IFPSShootable> _knownShootables = new System.Collections.Generic.Dictionary<Collider, IFPSShootable>();

        public delegate void ShotEventHandler();
        public delegate void TriedShotEventHandler(ShootInputResult result);
        public delegate void CartridgeLoadedEventHandler(bool result);
        public delegate void MagazineChangedEventHandler(FPSMagazine magazine);

        public event ShotEventHandler Shot;
        public event TriedShotEventHandler TriedShot;
        public event CartridgeLoadedEventHandler CartridgeLoaded;
        public event MagazineChangedEventHandler MagazineChanged;

        public enum ShootInputResult
        {
            None,
            Success,
            Reload,
            Failure
        }

        private FPSMagazine _fpsMagazine;
        public FPSMagazine FPSMagazine
        {
            get => _fpsMagazine;
            set
            {
                _fpsMagazine = value;
                MagazineChanged?.Invoke(_fpsMagazine);
            }
        }

        public string ConsoleProPrefix => "FPS Shoot";

        public bool ConsoleProMuted => _logsMuted;

        public bool TryLoadCartridge(Cartridge cartridge)
        {
            // Change to an enum if there're more than 2 possible results.
            bool result = FPSMagazine.TryLoadCartridge(cartridge);
            CartridgeLoaded?.Invoke(result);
            return result;
        }

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
            if (!_reloadInput && Input.GetButtonDown(INPUT_RELOAD))
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
            if (!_shootInput && Input.GetButtonDown(INPUT_SHOOT))
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
                        this.Log("Trying to shoot with a completely empty magazine.", gameObject);
                        _shootInput = false;
                        TriedShot?.Invoke(ShootInputResult.Failure);
                        return;
                    }

                    this.Log("Trying to shoot with an empty magazine load, reloading instead.", gameObject);
                    TriggerReload();
                    ResetShootInputDelay();
                    TriedShot?.Invoke(ShootInputResult.Reload);
                }
                else
                {
                    _isShooting = true;
                    _weaponAnimator.SetTrigger(ANM_PARAM_SHOOT);
                    ResetShootInputDelay();
                    TriedShot?.Invoke(ShootInputResult.Success);
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
            _isReloading = true;
            _weaponAnimator.SetTrigger(ANM_PARAM_RELOAD);
        }

        private void ApplyReload()
        {
            FPSMagazine.Reload();
        }

        private void ApplyShot()
        {
            if (!FPSMaster.DbgGodMode)
            {
                UnityEngine.Assertions.Assert.IsFalse(FPSMagazine.IsLoadEmpty, "Shoot animation has been allowed with an empty magazine load.");
                FPSMagazine.Shoot();
            }

            this.Log($"Applying shot. Magazine state : ({FPSMagazine.CurrLoadCount}/{FPSMagazine.CurrStorehouseCount}).", gameObject);

            RaycastHit[] hits = Physics.RaycastAll(_camTransform.position, _camTransform.forward, 100f, _shootableMask);
            if (hits.Length > 0)
            {
                System.Array.Sort(hits, delegate (RaycastHit hitA, RaycastHit hitB)
                {
                    return (hitA.point - _camTransform.position).sqrMagnitude.CompareTo((hitB.point - _camTransform.position).sqrMagnitude);
                });

                //string shotsInLineStr = "Shots in line : ";
                //for (int i = 0; i < hits.Length; ++i)
                //    shotsInLineStr += $"{hits[i].transform.name}{(i == hits.Length - 1 ? "" : " | ")}";
                //this.Log(shotsInLineStr, gameObject);

                RaycastHit hit;
                for (int i = 0; i < hits.Length; ++i)
                {
                    hit = hits[i];
                    this.Log($"Shot <b>{hit.transform.name}</b>.", hit.transform.gameObject);

                    if (!_knownShootables.TryGetValue(hit.collider, out IFPSShootable shootable))
                        if (hit.collider.TryGetComponent(out shootable))
                            _knownShootables.Add(hit.collider, shootable);

                    if (shootable != null)
                    {
                        shootable.OnShot(new FPSShotDatas(hit));
                        FPSMaster.FPSCameraShake.SetTrauma(shootable.TraumaOnShot);
                    }

                    // Bullet impacts.
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
                    {
                        Transform bulletImpactInstance = Instantiate(_bulletImpactPrefabs.Any(), hit.transform).transform;
                        bulletImpactInstance.position = hit.point + hit.normal * 0.01f;
                        bulletImpactInstance.forward = -hit.normal;
                        bulletImpactInstance.Rotate(0f, 0f, Random.Range(0, 4) * 90);

                        Vector3 scale = bulletImpactInstance.localScale;
                        scale.Scale(new Vector3(1f / hit.transform.localScale.x, 1f / hit.transform.localScale.y, 1f / hit.transform.localScale.z));
                        bulletImpactInstance.localScale = scale;
                    }

                    // Don't shoot on further hits if bullet can not cross the current one.
                    if (shootable == null || !shootable.ShotThrough)
                        break;
                }
            }

            Shot?.Invoke();
            FPSMaster.FPSCameraShake.AddTrauma(_shootTrauma);
        }

        private void UpdateAnimator()
        {
            _weaponAnimator.SetBool(ANM_PARAM_MOVING, FPSMaster.FPSController.CheckMovement());
            _weaponAnimator.SetBool(ANM_PARAM_SPRINTING, FPSMaster.FPSController.Sprinting);
            _weaponAnimator.SetBool(ANM_PARAM_CROUCHED, FPSMaster.FPSController.Crouched);
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
            if (Manager.ReferencesHub.Exists())
                Manager.ReferencesHub.OptionsManager.OptionsStateChanged += OnOptionsStateChanged;

            _weaponView.ShootAnimationOver += OnShootAnimationOver;
            _weaponView.ReloadAnimationOver += OnReloadAnimationOver;
            _weaponView.ShootFrame += OnShootFrame;
            _weaponView.ReloadFrame += OnReloadFrame;

            SetMagazine(_initMagazine);

            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("reload", "Reloads weapon.", () =>
            {
                if (FPSMagazine.CanReload && !_isShooting && !_isReloading)
                    TriggerReload();
            }));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand<int, int>("setMagazine", "Sets the weapon a new magazine.", SetMagazine));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("fulfillMagazine", "Fulfills the weapon magazine.", DBG_FulfillMagazine));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("emptyMagazine", "Empties the weapon magazine.", DBG_EmptyMagazine));
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand<int>("loadCartridge", "Loads a cartridge into the weapon magazine.", (capacity) =>
            {
                if (!TryLoadCartridge(new Cartridge(capacity)))
                    Console.DebugConsole.LogExternal("Magazine is already full.");
            }));
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

        private void DBG_FulfillMagazine()
        {
            FPSMagazine.Fulfill();
        }

        private void DBG_EmptyMagazine()
        {
            FPSMagazine.Empty();
        }
    }
}