// 오브젝트가 일정간격으로 생성되며 자식오브젝트가 애니메이션이 있으면 애니메이션 종료 후 생성됨
// 부모에서 자식오브젝트의 크기를 강제로 지정할 수 있음


using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StaggeredAnimatedDisplay : MonoBehaviour
{
    [Header("레이아웃")]
    public LayoutGroup layoutGroup;
    public bool resetOnStart = true;
    
    [Header("애니메이션")]
    public string animationName = "Show";
    public float fallbackDelay = 0.3f;

    [Header("사이즈 설정")]
    public bool overrideChildSize = false;
    public Vector2 childSize = new Vector2(100, 100);

    void Start()
    {
        if (resetOnStart)
        {
            foreach (Transform child in layoutGroup.transform)
            {
                child.gameObject.SetActive(false);
                var cg = child.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
            }
        }

        StartCoroutine(ShowChildrenSequentially());
    }

    IEnumerator ShowChildrenSequentially()
    {
        foreach (Transform child in layoutGroup.transform)
        {
            child.gameObject.SetActive(true);

            // ✅ 부모에서 자식 사이즈 제어
            if (overrideChildSize)
            {
                var rt = child.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = childSize;
            }

            // ✅ 자연스러운 재배치
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());

            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(animationName);

                yield return new WaitUntil(() =>
                    animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
                );
            }
            else
            {
                yield return new WaitForSeconds(fallbackDelay);
            }
        }
    }
}
