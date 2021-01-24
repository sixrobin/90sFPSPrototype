namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    public static class RigidbodyBumper
    {
        public static void Bump(Rigidbody rb, float force, float radius, float originYOffset)
        {
            Transform transform = rb.transform;

            float rndZ = Random.Range(0.5f, 1f);
            float rndX = Random.Range(0.5f, 1f);

            if (rb.transform.up.y <= 0f)
                rb.transform.up = rb.transform.up.WithY(1f);

            for (int i = 0; i < 3; ++i)
            {
                rb.AddExplosionForce(force, rb.transform.position.AddY(originYOffset), radius, 3f);
                rb.velocity = transform.TransformDirection(new Vector3(rndX, 10f, rndZ));
            }
        }
    }
}