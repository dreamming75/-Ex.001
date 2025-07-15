using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollDirectionAnimator : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Animation Clips (Optional)")]
    public AnimationClip clipLeft;
    public AnimationClip clipRight;
    public AnimationClip clipUp;
    public AnimationClip clipDown;

    [Header("Content and Threshold")]
    public Transform content;
    [Tooltip("Enable minimum drag distance check before playing animation")]
    [Header("최소 거리 체크 활성화 여부")]
    public bool enableThreshold = false;
    [Tooltip("Minimum drag (in pixels) required to trigger animation")]
    public float minDragDistance = 50f;

    private void Awake()
    {
        if (content == null && TryGetComponent<ScrollRect>(out var sr))
            content = sr.content;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // No-op; animations now play during dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;
        if (enableThreshold && delta.magnitude < minDragDistance)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
                PlayOnChildren(clipRight);
            else
                PlayOnChildren(clipLeft);
        }
        else
        {
            if (delta.y > 0)
                PlayOnChildren(clipUp);
            else
                PlayOnChildren(clipDown);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // No-op
    }

    private void PlayOnChildren(AnimationClip clip)
    {
        if (clip == null) return;

        foreach (Transform child in content)
        {
            var anim = child.GetComponent<Animation>();
            if (anim == null)
                anim = child.gameObject.AddComponent<Animation>();

            if (anim.GetClip(clip.name) == null)
                anim.AddClip(clip, clip.name);

            anim.clip = clip;
            anim.Play();
        }
    }
}
