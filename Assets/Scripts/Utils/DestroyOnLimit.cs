using UnityEngine;

namespace Utils
{
    public class DestroyOnLimit : MonoBehaviour
    {
        [SerializeField] private float _limitY = -10f;
        [SerializeField] private GameObject _target;
        private void Update()
        {
            if (transform.position.y < _limitY)
            {
                //TODO: If have time add a pool manager.
                Destroy(_target);
            }
        }
    }
}