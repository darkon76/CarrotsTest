using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarrotPuller
{
    public class Carrot: MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private enum CarrotState
        {
            ReadyToHarvest,
            Free,
        }

        private CarrotSpawner _carrotSpawner;
        
        private Plane _dragPlane;

        [Header("References")] 
        [SerializeField] private Rigidbody _rigidbody;
        [Header("Settings")] 
        [SerializeField] private float _groundDragOffset = .25f;
        [SerializeField] private float _dragInertiaMultiplier = 1f;

        [Header("Debug")] 
        [SerializeField] private CarrotState _currentState;
        [SerializeField] private float _forceApplied;
        [SerializeField] private bool _isDragging;
        [SerializeField] private bool _addDragInertia;
        [SerializeField] private Vector3 _dragTargetPosition;
        [SerializeField] private Vector3 _lastPosition;
        
        private void Awake()
        {
            _dragPlane = new Plane( Vector3.up, -_groundDragOffset );
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
        }

        public void Constructor(CarrotSpawner carrotSpawner)
        {
            _carrotSpawner = carrotSpawner;
        }


        public void OnDrag(PointerEventData eventData)
        {
            var ray = Camera.main.ScreenPointToRay( eventData.position);
            float enter;
            if( _dragPlane.Raycast( ray, out enter ) )
            {
                var rayPoint = ray.GetPoint(enter);
                _dragTargetPosition = rayPoint;
            }
            
            _isDragging = true;

            switch (_currentState)
            {
                case CarrotState.ReadyToHarvest:
                    break;
                case CarrotState.Free:
                    _rigidbody.useGravity = false;
                    break;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var ray = Camera.main.ScreenPointToRay( eventData.position);
            float enter;
            if (_dragPlane.Raycast(ray, out enter))
            {
                var rayPoint = ray.GetPoint(enter);
                _dragTargetPosition = rayPoint;
            }
            _isDragging = false;
            
            switch (_currentState)
            {
                case CarrotState.ReadyToHarvest:
                    break;
                case CarrotState.Free:
                    _rigidbody.useGravity = true;
                    _addDragInertia = true;
                    break;
            }
        }
        private void FixedUpdate()
        {
            if (_addDragInertia)
            {
                _rigidbody.velocity = (_dragTargetPosition - _lastPosition) * _dragInertiaMultiplier;
                _addDragInertia = false;
            }
            _lastPosition = _rigidbody.transform.position;
            if (!_isDragging)
            {
                return;
            }
                        
            switch (_currentState)
            {
                case CarrotState.ReadyToHarvest:
                    break;
                case CarrotState.Free:
                    FreeDrag();
                    break;
            }
        }
        private void FreeDrag()
        {
           _rigidbody.MovePosition(_dragTargetPosition);
        }
    }
}