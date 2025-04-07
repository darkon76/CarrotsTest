using System;
using SO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarrotPuller
{
    public class CropController: MonoBehaviour, IDragHandler,IBeginDragHandler, IEndDragHandler, IPointerClickHandler
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
        [SerializeField] private CropAnimatorHandler _animatorHandler;
        [SerializeField] private CropSettings _settings;
        [SerializeField] private Rigidbody _mainRB;
        [SerializeField] private Rigidbody[] _extraRBs;
        
        [Header("Debug")] 
        [SerializeField] private CropState _currentState = CropState.ReadyToHarvest;
        [SerializeField] private float _forceApplied;
        [SerializeField] private bool _isDragging;
        [SerializeField] private bool _addDragInertia;
        [SerializeField] protected Vector3 _dragTargetPosition;
        [SerializeField] private Vector3 _lastPosition;
        [SerializeField] private Quaternion _startingOrientation;
        
        public void Constructor(CropSpawner cropSpawner, int index)
        {
            _cropSpawner = cropSpawner;
            _fieldIndex = index;
        }

        
        private void Awake()
        {
            _dragPlane = new Plane( Vector3.up, - _settings.GroundDragOffset );

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
            switch (_currentState)
            {
                case CropState.ReadyToHarvest:
                    if (_isDragging)
                    {
                        var dragForce = Vector3.Magnitude(_dragTargetPosition- transform.position);
                        _forceApplied += dragForce;
                        _animatorHandler.PullingForce(dragForce);
                                    
                        if (_forceApplied >= _settings.TotalForceToHarvest)
                        {
                            CropHarvested();
                            _mainRB.useGravity = false;
                            _mainRB.isKinematic = true;
                        }
                    }
                    else
                    {
                        _forceApplied -= _settings.TotalForceDecayPerSecond * Time.deltaTime;
                        _forceApplied = Mathf.Max(0, _forceApplied);
                        transform.rotation = Quaternion.Slerp(transform.rotation, _startingOrientation, Time.deltaTime * 10f);
                    }
                    break;
                case CropState.Free:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {

            switch (_currentState)
            {
                case CropState.ReadyToHarvest:
                    break;
                case CropState.Free:
                    if (_addDragInertia)
                    {
                        var inertia = (_dragTargetPosition - _lastPosition) * _settings.DragInertiaMultiplier;
                        _mainRB.velocity = inertia;
                        _addDragInertia = false;
                    }

                    if (_isDragging)
                    {
                        _mainRB.position = _dragTargetPosition;
                        Debug.Log($"Dragging {_dragTargetPosition} d {_isDragging} k {_mainRB.isKinematic} g {_mainRB.useGravity}");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _lastPosition = _dragTargetPosition;
        }

        private void CropHarvested()
        {
            _currentState = CropState.Free;
            //TODO add particles and sound.
            if (_cropSpawner != null)
            {
                _cropSpawner.CropPulled(this);
            }
            _animatorHandler.Pulling(false);
            
            _mainRB.isKinematic = false;
            foreach (var rb in _extraRBs)
            {
                rb.isKinematic = false;
            }

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
            
            switch (_currentState)
            {
                case CropState.ReadyToHarvest:
                    //Rotate to drag point
                    transform.LookAt(_dragTargetPosition, Vector3.up);
                    break;
                case CropState.Free:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            if (_currentState == CropState.ReadyToHarvest)
            {
                _animatorHandler.Pulling(true);
            }
            else
            {
                _mainRB.useGravity = false;
                _mainRB.isKinematic = true;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            
            //We calculate the last position to use it as inertia.
            var ray = Camera.main.ScreenPointToRay( eventData.position);
            float enter;
            if (_dragPlane.Raycast(ray, out enter))
            {
                var rayPoint = ray.GetPoint(enter);
                _dragTargetPosition = rayPoint;
            }
            _animatorHandler.Pulling(false);
            switch (_currentState)
            {
                case CropState.ReadyToHarvest:
                    break;
                case CropState.Free:
                    _mainRB.useGravity = true;
                    _mainRB.isKinematic = false;
                    _addDragInertia = true;
                    break;
            }
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
                    _forceApplied += _settings.ClickHarvestForce;
                    if (_forceApplied >= _settings.TotalForceToHarvest)
                    {
                        CropHarvested();
                         _mainRB.useGravity = true;
                         _mainRB.AddForce(_settings.ClickForce, ForceMode.VelocityChange);
                    }
                    break;
                }
                case CropState.Free:
                    _mainRB.AddForce(_settings.ClickForce, ForceMode.VelocityChange);
                    break;
            }
        }
    }
}