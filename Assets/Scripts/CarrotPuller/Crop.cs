using UnityEngine;
using UnityEngine.EventSystems;

namespace CarrotPuller
{
    public class Crop: MonoBehaviour, IDragHandler,IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private enum CropState
        {
            ReadyToHarvest,
            Free,
        }

        private CropSpawner _cropSpawner;
        private int _fieldIndex;
        public int FieldsIndex => _fieldIndex;
        
        private Plane _dragPlane;

        [Header("References")] 
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private CropAnimatorHandler _animatorHandler;
        [Header("Settings")] 
        [SerializeField] private float _groundDragOffset = .25f;
        [SerializeField] private float _dragInertiaMultiplier = 1f;
        [Space]
        [SerializeField] private float _totalForceToHarvest = 50f;
        [SerializeField] private float _totalForceDecayPerSecond = 50f;
        [SerializeField] private Vector3 _harvestDragDirection = Vector3.forward;
        //Because for some small children is hard to drag, it was added support for clicking. 
        [SerializeField] private float _clickHarvestForce = 25f;
        [SerializeField] private Vector3 _clickFreeForce = new Vector3(0,100f,10f); //TODO use a better name.
        
        [Header("Debug")] 
        [SerializeField] private CropState _currentState;
        [SerializeField] private float _forceApplied;
        [SerializeField] private bool _isDragging;
        [SerializeField] private bool _addDragInertia;
        [SerializeField] private Vector3 _dragTargetPosition;
        [SerializeField] private Vector3 _lastPosition;
        [SerializeField] private Quaternion _startingOrientation;
        
        private void Awake()
        {
            _dragPlane = new Plane( Vector3.up, -_groundDragOffset );
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            if (_animatorHandler == null)
            {
                _animatorHandler = GetComponent<CropAnimatorHandler>();
            }
        }
        private void Start()
        {
            _startingOrientation = transform.rotation;
        }
        private void Update()
        {
            if (_currentState == CropState.ReadyToHarvest)
            {
                if (_isDragging)
                {
                    //TODO: Instead of drag target it should be at the crop plane to make it look nicer.
                    var dragForce = Vector3.Magnitude(_dragTargetPosition- transform.position);
                    _forceApplied += dragForce;
                    _animatorHandler.PullingForce(dragForce);
                                    
                    if (_forceApplied >= _totalForceToHarvest)
                    {
                        CarrotHarvested();
                    }
                }
                else // If not pulling the carrot 
                {
                    _forceApplied -= _totalForceDecayPerSecond * Time.deltaTime;
                    _forceApplied = Mathf.Max(0, _forceApplied);
                    transform.rotation = Quaternion.Slerp(transform.rotation, _startingOrientation, Time.deltaTime * 10f);
                }
            }
        }

        private void CarrotHarvested()
        {
            _currentState = CropState.Free;
            _rigidbody.isKinematic = false;
            //TODO add particles and sound.
            if (_cropSpawner != null)
            {
                _cropSpawner.CarrotPulled(this);
            }
            _animatorHandler.Pulling(false);
        }
        
        public void Constructor(CropSpawner cropSpawner, int index)
        {
            _cropSpawner = cropSpawner;
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
                case CropState.ReadyToHarvest:
                   
                    transform.LookAt(_dragTargetPosition, Vector3.up);
                    transform.Rotate(Vector3.right * 90);
                    break;
                case CropState.Free:
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
                case CropState.ReadyToHarvest:
                    _animatorHandler.Pulling(false);
                    break;
                case CropState.Free:
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
                case CropState.ReadyToHarvest:
                    break;
                case CropState.Free:
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
            _animatorHandler.Clicked();
            if (_isDragging)
            {
                return;
            }
            switch (_currentState)
            {
                case CropState.ReadyToHarvest:
                {
                    _forceApplied += _clickHarvestForce;
                    if (_forceApplied >= _totalForceToHarvest)
                    {
                        CarrotHarvested();
                        _rigidbody.useGravity = true;
                        _rigidbody.AddForce(_clickFreeForce, ForceMode.Force);
                    }
                    break;
                }
                    
                case CropState.Free:
                    _rigidbody.AddForce(_clickFreeForce, ForceMode.Force);
                    break;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentState == CropState.ReadyToHarvest)
            {
                _animatorHandler.Pulling(true);
            }
        }
    }
}