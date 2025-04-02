using System;
using UnityEngine;

namespace CarrotPuller
{
    public class CropAnimatorHandler : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        private int _tClickedHash = Animator.StringToHash("tClicked");
        private int _bPullingHash = Animator.StringToHash("bPulling");
        private int _fPullForceHash = Animator.StringToHash("fPullForce");

        //TODO:Add the juice. 
        //Events used to trigger particles/sounds.
        public event Action CropGround;
        public event Action FinishGrowing; //Notifies that the crop grow animation finished. 
        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        public void Clicked()
        {
            _animator.SetTrigger(_tClickedHash);
        }

        public void Pulling(bool isPulling)
        {
            _animator.SetBool(_bPullingHash, isPulling);
        }

        public void PullingForce(float pullingForce)
        {
            _animator.SetFloat(_fPullForceHash, pullingForce);
        }

        //Called from the animator
        private void OnCropGround()
        {
            CropGround?.Invoke();
        }

        private void OnFinishGrow()
        {
            FinishGrowing?.Invoke();
        }
    }
}