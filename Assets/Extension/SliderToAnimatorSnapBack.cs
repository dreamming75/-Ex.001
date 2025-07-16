using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class SliderToAnimatorSnapBack : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Slider slider;
    private Animator animator;
    private bool isDragging = false;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // ===== [슬라이더 자동 생성] ===== //
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        GameObject sliderGO = new GameObject("SnapSlider", typeof(RectTransform));
        sliderGO.transform.SetParent(canvas.transform, false);
        slider = sliderGO.AddComponent<Slider>();

        RectTransform rt = slider.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 50f);
        rt.sizeDelta = new Vector2(200f, 30f);

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        // 최소 UI: Background만 있어도 괜찮음
        GameObject bgGO = new GameObject("Background", typeof(Image));
        bgGO.transform.SetParent(sliderGO.transform, false);
        Image bgImage = bgGO.GetComponent<Image>();
        bgImage.color = Color.gray;
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImage;

        // 핸들
        GameObject handleGO = new GameObject("Handle", typeof(Image));
        handleGO.transform.SetParent(sliderGO.transform, false);
        Image handleImage = handleGO.GetComponent<Image>();
        handleImage.color = Color.white;
        RectTransform handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 20);
        slider.handleRect = handleRT;

        // 이벤트 연결
        slider.onValueChanged.AddListener(OnSliderChanged);

        // Pointer 이벤트 감지 위해 Raycast 가능하게
        sliderGO.AddComponent<CanvasRenderer>();
        sliderGO.AddComponent<Image>().color = Color.clear;

        // 이 스크립트도 슬라이더에 붙이기
        sliderGO.AddComponent<SliderToAnimatorSnapBack>();
    }

    void Start()
    {
        OnSliderChanged(slider.value);
    }

    void OnSliderChanged(float value)
    {
        if (isDragging)
        {
            animator.SetFloat("SliderValue", value);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if (Mathf.Approximately(slider.value, 1f))
        {
            animator.SetFloat("SliderValue", 1f);
        }
        else
        {
            slider.value = 0f;
            animator.SetFloat("SliderValue", 0f);
        }
    }
}
