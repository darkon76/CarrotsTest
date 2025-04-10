using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CarrotPuller
{
    public class CropSpawner : MonoBehaviour
    {
        [SerializeField] private CropController cropPrefab;
        [Space] 
        [Tooltip("Number of carrots that will be spawned at start up")]
        [Range(0, 6)] 
        [SerializeField] private int _startingNumberOfCarrots = 3;
        [Tooltip("The maximum number of carrots that can be on the screen at once. Should be equal or less than the number of spawn points")]
        [Range(2,6)]
        [SerializeField] private int _maxNumberOfCarrots = 3;
        [Tooltip("The minimum number of carrots that can be on the screen at once. Pulling a carrot will spawn a new one.")]
        [Range(0,5)]
        [SerializeField] private int _minNumberOfCarrots = 1;
        /// <summary>
        /// The points that the carrots will spawn at.
        /// Right now it is just a bool so there is the possibility that the user could pull a carrot and instantly a new one will spawn.
        /// If there is time I will add a cooldown to the spawn points.
        /// </summary>
        [SerializeField] private List<Transform> _carrotSpawnPoints;
        [Space]
        [SerializeField] private float _spawnCDSeconds = 2f;

        [Header("Debug")] 
        [SerializeField] private bool[] _freeSpawnPoints;
        [SerializeField] private int _currentNumberOfCarrots = 0;
        [SerializeField] private float _spawnTimer = 0f;

        private void OnValidate()
        {
            //TODO: Add validation for settings.
        }

        // Start is called before the first frame update
        private void Start()
        {
            _freeSpawnPoints = new bool[_carrotSpawnPoints.Count];
            for (int i = 0; i < _freeSpawnPoints.Length; i++)
            {
                _freeSpawnPoints[i] = true;
            }
            
            //TODO if there is time do a nicer sequence.
            for (int i = 0; i < _startingNumberOfCarrots; i++)
            {
                SpawnCrop(); 
            }
        }

        // Update is called once per frame
        private void Update()
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnCDSeconds)
            {
                _spawnTimer -= _spawnCDSeconds;
                SpawnCrop();
            }
        }
    
        private void SpawnCrop()
        {
            if (_currentNumberOfCarrots >= _maxNumberOfCarrots)
            {
                return;
            }

            var spawnPointsLength = _freeSpawnPoints.Length;
            var spawnPointIndex = Random.Range(0, spawnPointsLength);

            while (!_freeSpawnPoints[spawnPointIndex])
            {
                spawnPointIndex++;
                spawnPointIndex %= spawnPointsLength;
            }
        
            //TODO: If there is time add pooling

            var spawnPoint = _carrotSpawnPoints[spawnPointIndex];
            var crop = Instantiate(cropPrefab, spawnPoint.position, spawnPoint.rotation );
            crop.Constructor(this, spawnPointIndex);
            _currentNumberOfCarrots++;
            _freeSpawnPoints[spawnPointIndex] = false;
        }

        public void CropPulled(CropController crop)
        {
            _currentNumberOfCarrots--;
            _spawnTimer = 0;
            _freeSpawnPoints[crop.FieldsIndex] = true;
            if (_minNumberOfCarrots > _currentNumberOfCarrots)
            {
                SpawnCrop();
            }
        }
    }
}
