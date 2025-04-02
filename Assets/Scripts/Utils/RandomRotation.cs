using UnityEngine;

namespace Utils
{
    public class RandomRotation : MonoBehaviour
    {
        void Awake()
        {
            var randomAngle = Random.Range(0f, 360f);
            transform.Rotate(Vector3.up, randomAngle);
        }
    }
}
