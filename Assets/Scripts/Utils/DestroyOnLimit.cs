using UnityEngine;

namespace Utils
{
    public class DestroyOnLimit : MonoBehaviour
    {
        [SerializeField] private float _limitY = -10f;
        private void Update()
        {
            if (transform.position.y < _limitY)
            {
                Destroy(gameObject);
            }
        }
    }
}