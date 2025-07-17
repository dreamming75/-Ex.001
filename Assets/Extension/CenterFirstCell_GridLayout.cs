using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(ScrollRect))]
public class CenterNearestCell_GridLayout : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    public ScrollRect scrollRect;
    public float snapDuration = 0.3f;
    public float snapAnimationDelay = 0.2f;
    public AnimationClip snapAnimationClip;
    public AnimationClip dragAnimationClip;

    [Header("처음 중앙에 올 Cell 인덱스")]
    public int startCenterCellIndex = 0;

    private RectTransform currentCenterCell;
    private RectTransform snapActiveCell;
    private bool isDragging = false;
    private bool snapExecutedForThisStop = false;

    void Start()
    {
        StartCoroutine(CenterStartCellAnimated());
    }

    void OnEnable()
    {
        StartCoroutine(CenterStartCellAnimated());
    }

    IEnumerator CenterStartCellAnimated()
    {
        yield return null;

        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null || content.childCount == 0) yield break;

        Vector2 cellSize = grid.cellSize;
        Vector2 spacing = grid.spacing;
        Vector2 viewportSize = viewport.rect.size;

        // 가운데 정렬을 위한 padding 설정
        if (scrollRect.horizontal && !scrollRect.vertical)
        {
            float padding = (viewportSize.x - cellSize.x) / 2f;
            grid.padding.left = Mathf.RoundToInt(padding);
            grid.padding.right = Mathf.RoundToInt(padding);
        }
        else if (scrollRect.vertical && !scrollRect.horizontal)
        {
            float padding = (viewportSize.y - cellSize.y) / 2f;
            grid.padding.top = Mathf.RoundToInt(padding);
            grid.padding.bottom = Mathf.RoundToInt(padding);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        int targetIndex = Mathf.Clamp(startCenterCellIndex, 0, content.childCount - 1);
        RectTransform targetCell = content.GetChild(targetIndex) as RectTransform;
        if (targetCell == null) yield break;

        Vector2 viewportCenterWorld = viewport.position;
        Vector2 viewportCenterLocal = content.InverseTransformPoint(viewportCenterWorld);

        Vector2 cellLocalPos = targetCell.localPosition;
        Vector2 cellCenterLocal = cellLocalPos + new Vector2(
            targetCell.rect.width * (0.5f - targetCell.pivot.x),
            targetCell.rect.height * (0.5f - targetCell.pivot.y)
        );

        Vector2 offset = viewportCenterLocal - cellCenterLocal;
        Vector2 targetAnchoredPos = content.anchoredPosition + offset;

        Tween tween = null;
        scrollRect.inertia = false;

        if (scrollRect.horizontal && !scrollRect.vertical)
            tween = content.DOAnchorPosX(targetAnchoredPos.x, snapDuration);
        else if (scrollRect.vertical && !scrollRect.horizontal)
            tween = content.DOAnchorPosY(targetAnchoredPos.y, snapDuration);
        else
            tween = content.DOAnchorPos(targetAnchoredPos, snapDuration); // 양방향 지원

        if (tween != null)
        {
            tween.SetEase(Ease.OutCubic).OnComplete(() =>
            {
                scrollRect.inertia = true;
            });
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        snapExecutedForThisStop = false;

        if (snapActiveCell != null && snapActiveCell.childCount > 0)
        {
            var anim = snapActiveCell.GetChild(0).GetComponent<Animation>();
            if (anim != null) anim.Stop();
        }

        var centerCell = GetCenterCell();
        if (dragAnimationClip != null && centerCell != null && centerCell.childCount > 0)
        {
            var anim = centerCell.GetChild(0).GetComponent<Animation>();
            if (anim != null && anim.GetClip(dragAnimationClip.name) != null)
            {
                anim.Play(dragAnimationClip.name);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        SnapToNearestCell();
    }

    void Update()
    {
        RectTransform centerCell = GetCenterCell();
        bool isStopped = scrollRect.velocity.magnitude < 1f;
        bool centerChanged = currentCenterCell != null && centerCell != currentCenterCell;

        if (centerChanged)
        {
            if (snapActiveCell != null && snapActiveCell.childCount > 0)
            {
                var anim = snapActiveCell.GetChild(0).GetComponent<Animation>();
                if (anim != null) anim.Stop();
            }
            snapExecutedForThisStop = false;
        }

        if (centerCell != null && centerCell == currentCenterCell && isStopped && !isDragging && !snapExecutedForThisStop)
        {
            StartCoroutine(PlaySnapAnimationAfterDelay(centerCell));
            snapExecutedForThisStop = true;
        }

        currentCenterCell = centerCell;
    }

    IEnumerator PlaySnapAnimationAfterDelay(RectTransform centerCell)
    {
        yield return new WaitForSeconds(snapAnimationDelay);

        if (centerCell != currentCenterCell || isDragging) yield break;

        if (snapAnimationClip != null && centerCell.childCount > 0)
        {
            StopAllCellAnimationsExcept(centerCell);
            var anim = centerCell.GetChild(0).GetComponent<Animation>();
            if (anim != null && anim.GetClip(snapAnimationClip.name) != null)
            {
                anim.Play(snapAnimationClip.name);
            }
        }

        snapActiveCell = centerCell;
    }

    RectTransform GetCenterCell()
    {
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;
        Vector2 viewportCenterWorld = viewport.position;
        Vector2 viewportCenterLocal = content.InverseTransformPoint(viewportCenterWorld);

        float minDistance = float.MaxValue;
        RectTransform nearestCell = null;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform cell = content.GetChild(i) as RectTransform;
            if (cell == null) continue;

            Vector2 cellLocalPos = cell.localPosition;
            Vector2 cellCenterLocal = cellLocalPos + new Vector2(
                cell.rect.width * (0.5f - cell.pivot.x),
                cell.rect.height * (0.5f - cell.pivot.y)
            );

            float distance = (scrollRect.horizontal && !scrollRect.vertical)
                ? Mathf.Abs(cellCenterLocal.x - viewportCenterLocal.x)
                : Mathf.Abs(cellCenterLocal.y - viewportCenterLocal.y);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCell = cell;
            }
        }

        return nearestCell;
    }

    void SnapToNearestCell()
    {
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;
        if (content.childCount == 0) return;

        Vector2 viewportCenterWorld = viewport.position;
        Vector2 viewportCenterLocal = content.InverseTransformPoint(viewportCenterWorld);

        float minDistance = float.MaxValue;
        RectTransform nearestCell = null;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform cell = content.GetChild(i) as RectTransform;
            if (cell == null) continue;

            Vector2 cellLocalPos = cell.localPosition;
            Vector2 cellCenterLocal = cellLocalPos + new Vector2(
                cell.rect.width * (0.5f - cell.pivot.x),
                cell.rect.height * (0.5f - cell.pivot.y)
            );

            float distance = (scrollRect.horizontal && !scrollRect.vertical)
                ? Mathf.Abs(cellCenterLocal.x - viewportCenterLocal.x)
                : Mathf.Abs(cellCenterLocal.y - viewportCenterLocal.y);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCell = cell;
            }
        }

        if (nearestCell != null)
        {
            Vector2 cellLocalPos = nearestCell.localPosition;
            Vector2 cellCenterLocal = cellLocalPos + new Vector2(
                nearestCell.rect.width * (0.5f - nearestCell.pivot.x),
                nearestCell.rect.height * (0.5f - nearestCell.pivot.y)
            );

            Vector2 offset = viewportCenterLocal - cellCenterLocal;
            Vector2 targetAnchoredPos = content.anchoredPosition + offset;

            Tween tween = null;
            bool prevInertia = scrollRect.inertia;
            scrollRect.inertia = false;

            if (scrollRect.horizontal && !scrollRect.vertical)
                tween = content.DOAnchorPosX(targetAnchoredPos.x, snapDuration);
            else if (scrollRect.vertical && !scrollRect.horizontal)
                tween = content.DOAnchorPosY(targetAnchoredPos.y, snapDuration);
            else
                tween = content.DOAnchorPos(targetAnchoredPos, snapDuration);

            if (tween != null)
            {
                tween.SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    scrollRect.inertia = prevInertia;
                });
            }
        }
    }

    void StopAllCellAnimationsExcept(RectTransform exceptCell)
    {
        RectTransform content = scrollRect.content;
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform cell = content.GetChild(i) as RectTransform;
            if (cell == null || cell == exceptCell || cell.childCount == 0) continue;

            var anim = cell.GetChild(0).GetComponent<Animation>();
            if (anim != null) anim.Stop();
        }
    }
}
