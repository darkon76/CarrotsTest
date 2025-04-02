using UnityEngine;
using UnityEngine.EventSystems;

namespace CarrotPuller
{
    public class Carrot: MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private enum CarrotState
        {
            ReadyToHarvest,
            Free,
        }

        private CarrotSpawner _carrotSpawner;
        private int _fieldIndex;
        public int FieldsIndex => _fieldIndex;
        
        private Plane _dragPlane;

        [Header("References")] 
        [SerializeField] private Rigidbody _rigidbody;
        [Header("Settings")] 
        [SerializeField] private float _groundDragOffset = .25f;
        [SerializeField] private float _dragInertiaMultiplier = 1f;
        [Space]
        [SerializeField] private float _totalForceToHarvest = 50f;
        [SerializeField] private float _totalForceDecayPerSecond = 50f;
        [SerializeField] private Vector3 _harvestDragDirection = Vector3.forward;
        [SerializeField] private float _clickHarvestForce = 25f;
        [SerializeField] private Vector3 _clickHarvestPluckForce = new Vector3(0,100f,10f);
        
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
        private void Update()
        {
            if (_currentState == CarrotState.ReadyToHarvest)
            {
                if (_isDragging)
                {
                    var dragForce = Mathf.Abs(Vector3.Dot(_harvestDragDirection, _dragTargetPosition- transform.position));
                    _forceApplied += dragForce;
                                    
                    if (_forceApplied >= _totalForceToHarvest)
                    {
                        CarrotHarvested();
                    }
                }
                else // If not pulling the carrot 
                {
                    _forceApplied -= _totalForceDecayPerSecond * Time.deltaTime;
                    _forceApplied = Mathf.Max(0, _forceApplied);
                }

            }
        }

        private void CarrotHarvested()
        {
            _currentState = CarrotState.Free;
            _rigidbody.isKinematic = false;
            //TODO add particles and sound.
            if (_carrotSpawner != null)
            {
                _carrotSpawner.CarrotPulled(this);
            }
        }
        
        public void Constructor(CarrotSpawner carrotSpawner, int index)
        {
            _carrotSpawner = carrotSpawner;
            _fieldIndex = index;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (_currentState)
            {
                case CarrotState.ReadyToHarvest:
                {
                    _forceApplied += _clickHarvestForce;
                    if (_forceApplied >= _totalForceToHarvest)
                    {
                        CarrotHarvested();
                        _rigidbody.useGravity = true;
                        _rigidbody.AddForce(_clickHarvestPluckForce, ForceMode.Force);
                    }
                    break;
                }
                    
                case CarrotState.Free:
                    break;
            }
        }
    }
}