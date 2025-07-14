using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderAnimatorController : MonoBehaviour
{
    public Slider slider;
    public Animator animator;

    private float previousValue;
    private bool isAnimating = false;

    [SerializeField] private float minChangeThreshold = 0.01f; // 너무 미세한 변화는 무시
    [SerializeField] private float animationDuration = 0.5f;   // Increase/Decrease 애니메이션 클립 길이

    void Start()
    {
        previousValue = slider.value;
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float newValue)
    {
        if (isAnimating)
            return; // 애니메이션 재생 중이면 무시

        if (Mathf.Abs(newValue - previousValue) < minChangeThreshold)
            return; // 거의 변화 없음 → 무시

        // 이전 Trigger 리셋
        animator.ResetTrigger("Increase");
        animator.ResetTrigger("Decrease");

        if (newValue > previousValue)
        {
            animator.SetTrigger("Increase");
        }
        else if (newValue < previousValue)
        {
            animator.SetTrigger("Decrease");
        }

        previousValue = newValue;

        // 애니메이션 재생 중으로 설정
        StartCoroutine(WaitForAnimationEnd());
    }

    IEnumerator WaitForAnimationEnd()
    {
        isAnimating = true;
        yield return new WaitForSeconds(animationDuration); // 애니메이션 길이만큼 대기
        isAnimating = false;
    }
}
