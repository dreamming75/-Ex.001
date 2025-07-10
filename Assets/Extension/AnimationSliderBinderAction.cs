using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace H
{
    [RequireComponent(typeof(Slider))]
    public class AnimationSliderBinderAction : MonoBehaviour
    {
        [Header("애니메이션 컴포넌트가 있는 대상")]
        public Animation targetAnimation;

        [Header("다른 대상 애니메이션")]
        public Animation otherTargetAnimation; // 다른 오브젝트의 애니메이션

        [Header("Lerp 지속 시간 (초)")]
        public float duration = 0.1f;

        [Header("증가 애니메이션 클립")]
        public AnimationClip increaseAnimationClip;

        [Header("감소 애니메이션 클립")]
        public AnimationClip decreaseAnimationClip;

        private AnimationState _state = null;
        private Slider _slider;
        private bool _initialized = false;
        private Coroutine _animationCoroutine;
        private float lastValue = 0;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.minValue = 0;
            _slider.maxValue = 100;
            _slider.onValueChanged.AddListener(OnSliderChanged);

            Initialize();
        }

        private void Initialize()
        {
            if (targetAnimation == null || targetAnimation.clip == null)
            {
                Debug.LogError("대상 애니메이션 또는 클립이 없습니다.");
                return;
            }

            _state = targetAnimation[targetAnimation.clip.name];
            if (_state == null)
            {
                Debug.LogError("애니메이션 상태를 찾을 수 없습니다.");
                return;
            }

            _initialized = true;
        }

        private void OnSliderChanged(float value)
        {
            if (!_initialized) Initialize();
            if (_state == null) return;

            // 값이 증가하는지 감소하는지 체크
            bool isIncreasing = value > lastValue;

            // 값에 따라 애니메이션 실행
            if (isIncreasing)
            {
                PlayIncreaseAnimation();
            }
            else
            {
                PlayDecreaseAnimation();
            }

            lastValue = value;

            // 슬라이더 값에 맞춰 대상 애니메이션 실행
            float targetTime = Mathf.Clamp01(value / 100f);

            // 이전 애니메이션을 중지
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateToNormalizedTime(targetTime));
            
            // 다른 오브젝트의 애니메이션 실행
            TriggerOtherAnimation(isIncreasing);
        }

        private void PlayIncreaseAnimation()
        {
            // 증가 애니메이션이 설정되었으면 실행
            if (increaseAnimationClip != null)
            {
                Debug.Log("값이 증가했습니다! 증가 애니메이션 실행...");
                targetAnimation.Play(increaseAnimationClip.name);
            }
            else
            {
                Debug.LogWarning("증가 애니메이션 클립이 설정되지 않았습니다.");
            }
        }

        private void PlayDecreaseAnimation()
        {
            // 감소 애니메이션이 설정되었으면 실행
            if (decreaseAnimationClip != null)
            {
                Debug.Log("값이 감소했습니다! 감소 애니메이션 실행...");
                targetAnimation.Play(decreaseAnimationClip.name);
            }
            else
            {
                Debug.LogWarning("감소 애니메이션 클립이 설정되지 않았습니다.");
            }
        }

        private void TriggerOtherAnimation(bool isIncreasing)
        {
            if (otherTargetAnimation == null) return;

            if (isIncreasing)
            {
                // 값이 증가하면 다른 오브젝트의 애니메이션 실행
                if (increaseAnimationClip != null)
                {
                    Debug.Log("다른 오브젝트의 증가 애니메이션 실행...");
                    otherTargetAnimation.Play(increaseAnimationClip.name);
                }
            }
            else
            {
                // 값이 감소하면 다른 오브젝트의 애니메이션 실행
                if (decreaseAnimationClip != null)
                {
                    Debug.Log("다른 오브젝트의 감소 애니메이션 실행...");
                    otherTargetAnimation.Play(decreaseAnimationClip.name);
                }
            }
        }

        private IEnumerator AnimateToNormalizedTime(float targetNormalizedTime)
        {
            float start = _state.normalizedTime;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;  // UI라서 unscaledTime 추천
                float t = Mathf.Clamp01(elapsed / duration);
                float currentTime = Mathf.Lerp(start, targetNormalizedTime, t);

                _state.enabled = true;
                _state.weight = 1f;
                _state.normalizedTime = currentTime;
                targetAnimation.Sample();
                _state.enabled = false;

                yield return null;
            }

            // 마지막 값 보정
            _state.enabled = true;
            _state.normalizedTime = targetNormalizedTime;
            targetAnimation.Sample();
            _state.enabled = false;
        }
    }
}
