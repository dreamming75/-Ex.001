using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace H
{
    [RequireComponent(typeof(Slider))]
    public class AnimationSliderBinder : MonoBehaviour
    {
        [Header("Target with Animation Component")]
        public Animation targetAnimation;

        [Header("Lerp Duration (seconds)")]
        public float duration = 0.1f;

        private AnimationState _state = null;
        private Slider _slider;
        private bool _initialized = false;
        private Coroutine _animationCoroutine;

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
                Debug.LogError("Animation or clip is missing.");
                return;
            }

            _state = targetAnimation[targetAnimation.clip.name];
            if (_state == null)
            {
                Debug.LogError("AnimationState not found.");
                return;
            }

            _initialized = true;
        }

        private void OnSliderChanged(float value)
        {
            if (!_initialized) Initialize();
            if (_state == null) return;

            float targetTime = Mathf.Clamp01(value / 100f);

            // 이전에 실행 중이던 보간 멈춤
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateToNormalizedTime(targetTime));
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
