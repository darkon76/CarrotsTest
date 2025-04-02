using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils
{
    public class PerspectiveDrag: MonoBehaviour,  IDragHandler
    {
        private Camera _mainCamera;
        private Plane _plane;
        [SerializeField] float Offset = 0;
        [Header("Debug")]
        public bool IsEnabled = true;

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsEnabled)
            {
                return;
            }
            
            var ray = _mainCamera.ScreenPointToRay( eventData.position);
            float enter;
            if( _plane.Raycast( ray, out enter ) )
            {
                var rayPoint = ray.GetPoint(enter);
                transform.position = rayPoint;
            }
        }

        // Use this for initialization
        private void Awake()
        {
            _plane = new Plane( Vector3.up, -Offset );
            _mainCamera = Camera.main; //Cache the camera, micro optimization.
        }

        //If someone changes the offset.
        private void OnValidate()
        {
            _plane = new Plane( Vector3.up, -Offset );
        }
    }
}