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
        public Animation otherTargetAnimation;

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
            if (_initialized) return;

            if (targetAnimation == null)
            {
                Debug.LogError("🎯 [AnimationSliderBinderAction] 대상 애니메이션(targetAnimation)이 설정되지 않았습니다.");
                return;
            }

            if (targetAnimation.clip == null)
            {
                Debug.LogError("🎯 [AnimationSliderBinderAction] 대상 애니메이션 클립이 설정되지 않았습니다.");
                return;
            }

            _state = targetAnimation[targetAnimation.clip.name];
            if (_state == null)
            {
                targetAnimation.AddClip(targetAnimation.clip, targetAnimation.clip.name);
                _state = targetAnimation[targetAnimation.clip.name];
                if (_state == null)
                {
                    Debug.LogError($"🎯 [AnimationSliderBinderAction] 클립 '{targetAnimation.clip.name}' 을 대상 애니메이션에 등록할 수 없습니다.");
                    return;
                }
            }

            // 슬라이더용 대상에 클립 등록
            if (increaseAnimationClip != null && targetAnimation[increaseAnimationClip.name] == null)
                targetAnimation.AddClip(increaseAnimationClip, increaseAnimationClip.name);

            if (decreaseAnimationClip != null && targetAnimation[decreaseAnimationClip.name] == null)
                targetAnimation.AddClip(decreaseAnimationClip, decreaseAnimationClip.name);

            // 효과용 대상에도 클립 등록
            if (otherTargetAnimation != null)
            {
                if (increaseAnimationClip != null && otherTargetAnimation[increaseAnimationClip.name] == null)
                    otherTargetAnimation.AddClip(increaseAnimationClip, increaseAnimationClip.name);

                if (decreaseAnimationClip != null && otherTargetAnimation[decreaseAnimationClip.name] == null)
                    otherTargetAnimation.AddClip(decreaseAnimationClip, decreaseAnimationClip.name);
            }

            _initialized = true;
        }

        private void OnSliderChanged(float value)
        {
            if (!_initialized) Initialize();
            if (_state == null) return;

            bool isIncreasing = value > lastValue;

            // 증감 효과는 다른 대상에서 실행
            if (isIncreasing)
                PlayIncreaseAnimation();
            else
                PlayDecreaseAnimation();

            lastValue = value;

            float targetTime = Mathf.Clamp01(value / 100f);

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateToNormalizedTime(targetTime));
        }

        private void PlayIncreaseAnimation()
        {
            if (increaseAnimationClip != null && otherTargetAnimation != null)
            {
                Debug.Log("값이 증가했습니다! 증가 애니메이션 실행 (다른 오브젝트)...");
                otherTargetAnimation.Play(increaseAnimationClip.name);
            }
            else
            {
                Debug.LogWarning("증가 애니메이션 클립 또는 다른 대상 애니메이션이 설정되지 않았습니다.");
            }
        }

        private void PlayDecreaseAnimation()
        {
            if (decreaseAnimationClip != null && otherTargetAnimation != null)
            {
                Debug.Log("값이 감소했습니다! 감소 애니메이션 실행 (다른 오브젝트)...");
                otherTargetAnimation.Play(decreaseAnimationClip.name);
            }
            else
            {
                Debug.LogWarning("감소 애니메이션 클립 또는 다른 대상 애니메이션이 설정되지 않았습니다.");
            }
        }

        private IEnumerator AnimateToNormalizedTime(float targetNormalizedTime)
        {
            float start = _state.normalizedTime;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float currentTime = Mathf.Lerp(start, targetNormalizedTime, t);

                _state.enabled = true;
                _state.weight = 1f;
                _state.normalizedTime = currentTime;
                targetAnimation.Sample();
                _state.enabled = false;

                yield return null;
            }

            _state.enabled = true;
            _state.normalizedTime = targetNormalizedTime;
            targetAnimation.Sample();
            _state.enabled = false;
        }
    }
}