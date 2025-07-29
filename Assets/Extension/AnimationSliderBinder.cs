using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace H
{
    [RequireComponent(typeof(Slider))]
    public class AnimationSliderBinder : MonoBehaviour
    {
        [Header("Target with Animation Component")]
        public Animation targetAnimation;

        [Header("Text Display")]
        public TextMeshProUGUI valueText; // 슬라이더 값을 표시할 TextMeshProUGUI 컴포넌트
        public int decimalPlaces = 1; // 소수점 자릿수
        public bool showPercentage = true; // % 기호 표시 여부

        [Header("Current/Total Value Display")]
        public TextMeshProUGUI currentValueText; // 현재값을 표시할 TextMeshProUGUI 컴포넌트
        public TextMeshProUGUI totalValueText; // 전체값을 표시할 TextMeshProUGUI 컴포넌트
        public float totalValue = 100f; // 전체값 설정

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
            
            // 초기 Text 값 설정
            UpdateValueText(_slider.value);
            UpdateTotalValueText();
        }

        private void OnSliderChanged(float value)
        {
            if (!_initialized) Initialize();
            if (_state == null) return;

            // Text 값 업데이트
            UpdateValueText(value);

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

        private void UpdateValueText(float value)
        {
            if (valueText != null)
            {
                string format = "F" + decimalPlaces.ToString();
                string displayText = value.ToString(format);
                if (showPercentage)
                {
                    displayText += "%";
                }
                valueText.text = displayText;
            }

            // 현재값 텍스트 업데이트
            if (currentValueText != null)
            {
                string format = "F" + decimalPlaces.ToString();
                float currentValue = (value / 100f) * totalValue;
                currentValueText.text = currentValue.ToString(format);
            }
        }

        private void UpdateTotalValueText()
        {
            if (totalValueText != null)
            {
                string format = "F" + decimalPlaces.ToString();
                totalValueText.text = totalValue.ToString(format);
            }
        }
    }
}
