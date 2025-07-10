using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SliderResetter : MonoBehaviour, IEndDragHandler
{
    public Slider slider;
    public float resetDuration = 0.3f;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Animator animator;

    private Coroutine resetCoroutine;
    private float threshold = 0.01f;

    public void OnEndDrag(PointerEventData eventData)
    {
        bool isAtMax = Mathf.Abs(slider.value - slider.maxValue) < threshold;

        if (isAtMax)
        {
            // 최대값 도달 시: 슬라이더 비활성화
            slider.interactable = false;

            // 필요 시 애니메이터에 별도 트리거 전송 가능
            // animator?.SetTrigger("Lock");
            return;
        }

        if (slider.value > 0f)
        {
            if (resetCoroutine != null)
                StopCoroutine(resetCoroutine);

            resetCoroutine = StartCoroutine(ResetSliderValue());

            animator?.SetTrigger("Reset");
        }
    }

    IEnumerator ResetSliderValue()
    {
        float startValue = slider.value;
        float time = 0f;

        while (time < resetDuration)
        {
            time += Time.deltaTime;
            float t = time / resetDuration;
            float easedT = easingCurve.Evaluate(t);
            slider.value = Mathf.Lerp(startValue, 0f, easedT);
            yield return null;
        }

        slider.value = 0f;
    }
}
