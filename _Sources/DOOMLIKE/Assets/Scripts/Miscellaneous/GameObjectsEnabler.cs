namespace Doomlike
{
    using UnityEngine;

    public class GameObjectsEnabler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _toEnable = null;

        private void Awake()
        {
            if (GetComponent<Collider>() == null)
                Debug.LogWarning("GameObjectsEnabler WARNING: gameObject doesn't have a collider and can't be interacted.", gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            for (int i = _toEnable.Length - 1; i >= 0; --i)
                _toEnable[i].SetActive(true);

            gameObject.SetActive(false);
        }
    }
}