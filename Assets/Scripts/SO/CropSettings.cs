using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "CropSettings", menuName = "ScriptableObjects/CropSettings", order = 1)]
    public class CropSettings: ScriptableObject
    {
        [Header("Harvest")] 
        [SerializeField] private float _totalForceToHarvest = 100f;
        [SerializeField] private float _totalForceDecayPerSecond = 50f;
        [SerializeField] private float _pullPlaneOffset = .1f;
        [Header("Drag")]
        [SerializeField] private float _groundDragOffset = .25f;
        [SerializeField] private float _dragInertiaMultiplier = 5f;
        [Header("Click")]
        [SerializeField] private float _clickHarvestForce = 25f;
        [SerializeField] private Vector3 _clickForce = new Vector3(0,100f,10f);
        
        
        public float GroundDragOffset => _groundDragOffset;
        public float DragInertiaMultiplier => _dragInertiaMultiplier;
        public float TotalForceToHarvest => _totalForceToHarvest;
        public float TotalForceDecayPerSecond => _totalForceDecayPerSecond;
        public float ClickHarvestForce => _clickHarvestForce;
        public Vector3 ClickForce => _clickForce;
        public float PullPlaneOffset => _pullPlaneOffset;
        
    }
}