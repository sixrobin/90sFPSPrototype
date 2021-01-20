namespace Doomlike
{
    public class PlaceholderReward : OutlinedInteraction
    {
        public override void Interact()
        {
            base.Interact();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            transform.Rotate(0f, 135f * UnityEngine.Time.deltaTime, 0f, UnityEngine.Space.World);
        }
    }
}