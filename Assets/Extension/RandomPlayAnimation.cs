using System.Collections.Generic;
using UnityEngine;

namespace H.Library.Components
{
    [RequireComponent(typeof(Animation))]
    public class RandomPlayAnimation : MonoBehaviour
    {
        [Header("Start에서 자동 재생할지 여부")]
        public bool playOnStart = true; // 
        [Header("애니메이션 완료 후 랜덤 재생할지 여부")]
        public bool loopRandomly = true; // 
        [Header("같은 애니메이션 연속 재생 방지")]
        public bool avoidSameAnimation = true; // 

        private Animation _anim = null;
        private List<string> _animationList = new List<string>();
        private string _lastPlayedAnimation = ""; // 마지막에 재생된 애니메이션
        private bool _isPlaying = false;

        private void Awake()
        {
            _anim = GetComponent<Animation>();
            InitializeAnimationList();
        }

        private void InitializeAnimationList()
        {
            _animationList.Clear();
            
            if (_anim == null)
            {
                Debug.LogError("Animation component not found on " + gameObject.name);
                return;
            }

            foreach (AnimationState state in _anim)
            {
                if (state.clip != null)
                {
                    _animationList.Add(state.name);
                }
            }

            if (_animationList.Count == 0)
            {
                Debug.LogWarning("No animation clips found on " + gameObject.name);
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        private void OnEnable()
        {
            // 오브젝트가 활성화될 때마다 랜덤 애니메이션 실행
            if (_animationList.Count > 0)
            {
                Play();
            }
        }

        private void Update()
        {
            // 애니메이션 완료 후 랜덤 재생
            if (loopRandomly && _isPlaying && !_anim.isPlaying && _animationList.Count > 0)
            {
                Play();
            }
        }

        public void Play()
        {
            if (_animationList.Count == 0)
            {
                Debug.LogWarning("No animations available to play on " + gameObject.name);
                return;
            }

            string animationToPlay = GetRandomAnimation();
            
            if (string.IsNullOrEmpty(animationToPlay))
            {
                Debug.LogWarning("Failed to select animation on " + gameObject.name);
                return;
            }

            _anim.Play(animationToPlay);
            _lastPlayedAnimation = animationToPlay;
            _isPlaying = true;
        }

        private string GetRandomAnimation()
        {
            if (_animationList.Count == 0) return "";

            // 같은 애니메이션 연속 재생 방지
            if (avoidSameAnimation && _animationList.Count > 1 && !string.IsNullOrEmpty(_lastPlayedAnimation))
            {
                List<string> availableAnimations = new List<string>(_animationList);
                availableAnimations.Remove(_lastPlayedAnimation);
                
                int randomIndex = Random.Range(0, availableAnimations.Count);
                return availableAnimations[randomIndex];
            }
            else
            {
                int randomIndex = Random.Range(0, _animationList.Count);
                return _animationList[randomIndex];
            }
        }

        public void Stop()
        {
            _anim.Stop();
            _isPlaying = false;
        }

        public void Pause()
        {
            _anim.enabled = false;
        }

        public void Resume()
        {
            _anim.enabled = true;
        }

        // 애니메이션 리스트 새로고침 (런타임에 애니메이션이 추가/제거된 경우)
        public void RefreshAnimationList()
        {
            InitializeAnimationList();
        }

        // 현재 재생 중인 애니메이션 이름 반환
        public string GetCurrentAnimationName()
        {
            if (_anim.isPlaying)
            {
                foreach (AnimationState state in _anim)
                {
                    if (state.enabled)
                    {
                        return state.name;
                    }
                }
            }
            return "";
        }
    }
}