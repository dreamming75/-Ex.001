using UnityEngine;
using UnityEngine.UI;

public class SliderToAnimator : MonoBehaviour
{
    public Slider slider;        // 슬라이더 연결
    public Animator animator;    // 애니메이터 연결

    void Start()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
        OnSliderChanged(slider.value); // 시작 시 반영
    }

    void OnSliderChanged(float value)
    {
        animator.SetFloat("SliderValue", value);
    }
}
