using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SnapScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float duration = 0.3f;
    public Ease easeType = Ease.OutCubic;

    private float cellStep = 200f; // 한 셀(셀+간격)의 크기
    private bool isHorizontal = true;
    private bool isVertical = false;

    private void Start()
    {
        var grid = content.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            cellStep = scrollRect.horizontal ? (grid.cellSize.x + grid.spacing.x) : (grid.cellSize.y + grid.spacing.y);
        }
        else
        {
            Debug.LogWarning("GridLayoutGroup이 없습니다! cellStep=100");
            cellStep = 100f;
        }

        isHorizontal = scrollRect.horizontal;
        isVertical = scrollRect.vertical;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        content.DOKill();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그를 놓는 순간 바로 스냅
        SnapToClosestCell();
    }

    private System.Collections.IEnumerator SnapWhenStopped()
    {
        // 스크롤이 멈출 때까지 대기
        while (scrollRect.velocity.magnitude > 10f) // 10f는 임계값, 필요시 조정
        {
            yield return null;
        }
        SnapToClosestCell();
    }

    void SnapToClosestCell()
    {
        // 방향에 따라 계산
        if (isHorizontal)
        {
            float currentX = content.anchoredPosition.x;
            int nearestIndex = Mathf.RoundToInt(currentX / cellStep);
            float targetX = nearestIndex * cellStep;
            scrollRect.velocity = Vector2.zero;
            content.DOAnchorPosX(targetX, duration).SetEase(easeType);
        }
        else if (isVertical)
        {
            float currentY = content.anchoredPosition.y;
            int nearestIndex = Mathf.RoundToInt(currentY / cellStep);
            float targetY = nearestIndex * cellStep;
            scrollRect.velocity = Vector2.zero;
            content.DOAnchorPosY(targetY, duration).SetEase(easeType);
        }
    }
}
