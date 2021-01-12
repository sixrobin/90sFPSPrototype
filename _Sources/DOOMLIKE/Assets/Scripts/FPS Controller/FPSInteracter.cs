﻿namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    /// <summary>
    /// Main class for the FPS interaction controller. Raycasts to the camera forward to handle interactions.
    /// Can be enabled/disabled if needed.
    /// </summary>
    public class FPSInteracter : FPSControllableComponent, IConsoleProLoggable
    {
        [Header("REFERENCES")]
        [SerializeField] private Transform _camTransform = null;

        [Header("SETTINGS")]
        [SerializeField] private float _maxDist = 1.5f;
        [SerializeField] private LayerMask _interactionMask = 0;

        [Header("DEBUG")]
        [SerializeField] private bool _dbg = true;

        private FPSInteraction _lastInteracted;

        private FPSInteraction _currentInteraction;
        private FPSInteraction CurrentInteraction
        {
            get => _currentInteraction;
            set
            {
                _currentInteraction = value;
                if (_currentInteraction != null)
                {
                    ConsoleProLogger.Log(this, $"Focusing <b>{_currentInteraction.transform.name}</b>.", _currentInteraction.gameObject);
                    _currentInteraction.Focus();
                }
            }
        }

        public bool InteractedThisFrame { get; private set; }

        public string ConsoleProPrefix => "FPS Interacter";

        protected override void OnControlAllowed()
        {
            base.OnControlAllowed();
        }

        protected override void OnControlDisallowed()
        {
            base.OnControlDisallowed();
            ResetCurrentInteractions();
        }

        /// <summary>
        /// Resets all interactions variables.
        /// </summary>
        private void ResetCurrentInteractions()
        {
            if (CurrentInteraction != null)
            {
                ConsoleProLogger.Log(this, $"Unfocusing <b>{CurrentInteraction.transform.name}</b>.", CurrentInteraction.gameObject);
                CurrentInteraction.Unfocus();
            }

            CurrentInteraction = null;
            _lastInteracted = null;
        }

        /// <summary>
        /// Checks if an interactable gameObject is in front of the camera, and sets it as the current interaction
        /// after some more checks to make sure the interaction is allowed.
        /// </summary>
        private void CheckForInteraction()
        {
            if (Physics.Raycast(_camTransform.position, _camTransform.forward, out RaycastHit hit, _maxDist, _interactionMask))
            {
                if (_dbg)
                    Debug.DrawLine(_camTransform.position, hit.point, Color.red);

                if (CurrentInteraction == null
                    && hit.collider.TryGetComponent(out FPSInteraction interactable)
                    && interactable.InteractionAllowed
                    && interactable != _lastInteracted)
                    CurrentInteraction = interactable;
            }
            else
            {
                if (_dbg)
                    Debug.DrawLine(_camTransform.position, _camTransform.position + _camTransform.forward * _maxDist, Color.red);

                ResetCurrentInteractions();
            }
        }

        /// <summary>
        /// Triggers the current interaction if it exists and the players inputs the interaction button.
        /// </summary>
        private void TryInteract()
        {
            if (CurrentInteraction == null)
                return;

            if (!InteractedThisFrame && Input.GetButtonDown("Interact"))
            {
                ConsoleProLogger.Log(this, $"Interacting with <b>{transform.name}</b>.", gameObject);

                CurrentInteraction.Interact();
                if (CurrentInteraction && CurrentInteraction.UnfocusedOnInteracted)
                {
                    _lastInteracted = CurrentInteraction;
                    CurrentInteraction = null;
                }

                InteractedThisFrame = true;
                StartCoroutine(ResetInteractedOnEndOfFrameCoroutine());
            }
        }

        private System.Collections.IEnumerator ResetInteractedOnEndOfFrameCoroutine()
        {
            yield return new WaitForEndOfFrame();
            InteractedThisFrame = false;
        }

        private void Update()
        {
            if (!Controllable)
                return;

            CheckForInteraction();
            TryInteract();
        }

        private void OnGUI()
        {
            if (!_dbg || !Manager.DebugManager.DbgViewOn)
                return;

            GUI.Label(new Rect(10, 10, 500, 20), CurrentInteraction != null ? $"Can interact with {CurrentInteraction.transform.name}." : "No interaction possible.");
        }
    }
}