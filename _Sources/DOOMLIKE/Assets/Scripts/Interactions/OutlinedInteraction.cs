namespace Doomlike
{
    using UnityEngine;

    public class OutlinedInteraction : FPSSystem.FPSInteraction
    {
        [SerializeField] private cakeslice.Outline _outline = null;

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

        protected override void Awake()
        {
            base.Awake();
            _outline.eraseRenderer = true;
        }
    }
}