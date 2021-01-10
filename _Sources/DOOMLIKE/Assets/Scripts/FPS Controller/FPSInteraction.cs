namespace Doomlike.FPSCtrl
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Abstract class for every gameObject in the game that the FPS controller can interact with.
    /// </summary>
    public abstract class FPSInteraction : MonoBehaviour
    {
        [SerializeField] bool _unfocusedOnInteracted = false;
        [SerializeField] bool _uniqueInteraction = false;
        [SerializeField] UnityEvent _onInteracted = null;

        public delegate void InteractedEventHandler(FPSInteraction interaction);

        public event InteractedEventHandler Interacted;

        public bool InteractionAllowed { get; private set; } = true;

        public bool UnfocusedOnInteracted => _unfocusedOnInteracted;

        /// <summary>
        /// Called when the player is looking to the gameObject and is ready to interact with it.
        /// </summary>
        public virtual void Focus()
        {
            if (!InteractionAllowed)
                return;
        }

        /// <summary>
        /// Called with the player leaves the interactable gameObject.
        /// </summary>
        public virtual void Unfocus()
        {
        }

        /// <summary>
        /// Called when the player interacts with the gameObject.
        /// Calls related events after some more checks.
        /// </summary>
        public virtual void Interact()
        {
            if (!InteractionAllowed)
                return;

            if (_uniqueInteraction)
                InteractionAllowed = false;

            if (_unfocusedOnInteracted)
                Unfocus();

            Interacted?.Invoke(this);
            _onInteracted.Invoke();
        }

        public void SetInteractionAvailability(bool state)
        {
            InteractionAllowed = state;
        }

        protected virtual void Awake()
        {
            // No RequireComponent because the collider type is variable.
            if (!GetComponent<Collider>())
                Debug.LogWarning("FPSInteraction WARNING: gameObject doesn't have a collider and can't be interacted.", gameObject);
        }
    }
}