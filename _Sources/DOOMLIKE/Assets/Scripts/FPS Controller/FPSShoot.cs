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

        [Header("IMPACT")]
        [SerializeField] private GameObject[] _bulletImpactPrefabs = null;
        [SerializeField] private float _shootTrauma = 0.15f;

        private bool _canShoot = true;
        private bool _isShooting; // Animating running.

        private System.Collections.Generic.Dictionary<Collider, IFPSShootable> _knownShootables = new System.Collections.Generic.Dictionary<Collider, IFPSShootable>();

        public delegate void ShotEventHandler();

        public event ShotEventHandler Shot;

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

        private void TryShoot()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                ConsoleProLogger.Log(this, "Triggering shoot animation.", gameObject);
                _isShooting = true;
                _weaponAnimator.SetTrigger("Shoot");
            }
        }

        private void ApplyShoot()
        {
            ConsoleProLogger.Log(this, "Apply shot.", gameObject);

            if (Physics.Raycast(_camTransform.position, _camTransform.forward, out RaycastHit hit, Mathf.Infinity))
            {
                ConsoleProLogger.Log(this, $"Shot on <b>{hit.transform.name}</b>.", gameObject);

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

        private void OnOptionsStateChanged(bool state)
        {
            _canShoot = !state;
        }

        private void OnShootAnimationOver()
        {
            _isShooting = false;
        }

        private void OnShootFrame()
        {
            ApplyShoot();
        }

        private void Start()
        {
            Manager.ReferencesHub.OptionsManager.OptionsStateChanged += OnOptionsStateChanged;
            _weaponView.ShootAnimationOver += OnShootAnimationOver;
            _weaponView.ShootFrame += OnShootFrame;
        }

        private void Update()
        {
            if (!Controllable || !_canShoot || _isShooting)
                return;

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