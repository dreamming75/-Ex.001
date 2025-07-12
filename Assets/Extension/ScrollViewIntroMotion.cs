//스크롤뷰가 켜질 때 Content 시작 위치를 지정해서 이동 애니미에션



using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollViewIntroMotion : MonoBehaviour
{
    public ScrollRect scrollRect;                // ScrollView 참조
    public Vector2 startPosition = new Vector2(0, -500); // 시작 위치
    public float moveDuration = 1f;              // 이동 시간
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 부드러운 커브

    private RectTransform content;
    private Vector2 targetPosition;

    void OnEnable()
    {
        if (scrollRect == null)
        {
            Debug.LogWarning("ScrollRect가 할당되지 않았습니다.");
            return;
        }

        content = scrollRect.content;
        targetPosition = content.anchoredPosition;

        // 시작 위치로 설정
        content.anchoredPosition = startPosition;

        // 이동 코루틴 시작
        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        float elapsed = 0f;
        Vector2 initialPosition = content.anchoredPosition;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float curvedT = moveCurve.Evaluate(t);

            content.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, curvedT);
            yield return null;
        }

        content.anchoredPosition = targetPosition;
    }
}
