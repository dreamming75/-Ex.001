using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SliderResetterAnimator : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Slider slider;
    public float resetDuration = 0.3f;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Animator animator;

    private Coroutine resetCoroutine;
    private float threshold = 0.01f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작
        animator?.SetBool("Dragging", true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 실시간 드래그 감지 시 로직 확장 가능 (예: 사운드)
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        animator?.SetBool("Dragging", false); // 드래그 종료 시 해제

        float value = slider.value;
        float max = slider.maxValue;
        float min = slider.minValue;

        bool isAtMax = Mathf.Abs(value - max) < threshold;
        bool isAtMin = Mathf.Abs(value - min) < threshold;

        if (isAtMax)
        {
            slider.interactable = false;
            animator?.SetTrigger("Max");
            return;
        }

        if (isAtMin)
        {
            animator?.SetTrigger("Zero");
            return;
        }

        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetSliderValue());
        animator?.SetTrigger("Reset");
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
