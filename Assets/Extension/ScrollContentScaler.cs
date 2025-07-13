using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  // DOTween 네임스페이스

[RequireComponent(typeof(ScrollRect))]
public class ScrollContentScaler : MonoBehaviour
{
    public RectTransform content;
    
    [Header("스크롤 방향")]
    public bool isHorizontal = false; // false면 Vertical로 동작

    [Header("스케일 범위")]
    public Vector3 startScale = Vector3.one * 0.5f;
    public Vector3 endScale = Vector3.one;

    [Header("스케일 변화 커브")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("애니메이션")]
    public float tweenDuration = 0.2f; // 스케일이 변경될 때 DOTween 애니메이션 시간

    private ScrollRect scrollRect;
    private Vector3 currentTargetScale;

    private Tweener scaleTweener;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    void Update()
    {
        float rawT = isHorizontal 
            ? scrollRect.horizontalNormalizedPosition
            : 1f - scrollRect.verticalNormalizedPosition;

        float curvedT = scaleCurve.Evaluate(rawT);
        Vector3 targetScale = Vector3.Lerp(startScale, endScale, curvedT);

        // DOTween으로 스무스하게 변화시키기 (중복 방지)
        if (currentTargetScale != targetScale)
        {
            currentTargetScale = targetScale;

            if (scaleTweener != null && scaleTweener.IsActive())
                scaleTweener.Kill();

            scaleTweener = content.DOScale(targetScale, tweenDuration)
                                  .SetEase(Ease.OutQuad);
        }
    }
}
