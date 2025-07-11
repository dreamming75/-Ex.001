using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class CardFanEffect : MonoBehaviour
{
    [Header("카드 배열 설정")]
    public float radius = 300f;
    public float maxAngle = 60f;
    public float animationDuration = 0.5f;
    public bool faceCenter = true;

    [Header("초기 연출 설정")]
    public bool autoUnfoldOnEnable = true;
    public float startDelay = 0.3f;
    public float initialSpacing = 30f;

    [Header("펼치기 딜레이")]
    public float fanoutDelay = 0f;
    public bool useSequentialDelay = true;
    public float perCardDelay = 0.05f;

    [Header("펼쳐질 때 애니메이션 커브")]
    public Ease positionEase = Ease.OutSine;
    public Ease rotationEase = Ease.OutSine;
    public Ease scaleEase = Ease.OutBack;

    [Header("펼쳐질 때 스케일 설정")]
    public bool useScaleAnimation = true;
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
    public Vector3 endScale = Vector3.one;

    [Header("펼쳐질 때 간격 보정 (X축만)")]
    [Range(0.1f, 3f)] public float spacingMultiplier = 1f;

    [Header("접기 설정")]
    public Vector2 foldAnchoredPosition = Vector2.zero;
    public Vector3 foldEulerRotation = Vector3.zero;

    [Header("애니메이션 클립 이름")]
    public string startClipName = "Start";
    public string unfoldStartClipName = "Unfold_Start";
    public string unfoldEndClipName = "Unfold_End";
    public string foldEndClipName = "Fold_End";

    [Header("고정 간격 설정")]
    public bool useFixedAngleStep = false;
    public float fixedAngleStep = 15f;

    private List<RectTransform> cards = new List<RectTransform>();
    private bool isFolded = false;

    void OnEnable()
    {
        RefreshCardList();
        ResetCardsToStartSpacing();

        foreach (var card in cards)
            PlayAnimation(card, startClipName);

        if (autoUnfoldOnEnable)
            Invoke(nameof(Unfold), startDelay);
    }

    void RefreshCardList()
    {
        cards.Clear();
        foreach (RectTransform t in GetComponentsInChildren<RectTransform>())
        {
            if (t.parent == transform)
                cards.Add(t);
        }
    }

    void ResetCardsToStartSpacing()
    {
        int count = cards.Count;
        if (count == 0) return;

        float step = (count > 1) ? initialSpacing / (count - 1) : 0f;
        float startX = -initialSpacing / 2f;

        for (int i = 0; i < count; i++)
        {
            var card = cards[i];
            card.DOKill();
            card.anchoredPosition = new Vector2(startX + i * step, 0f);
            card.localRotation = Quaternion.identity;
            card.localScale = useScaleAnimation ? startScale : endScale;
            LockChildLocalTransform(card);
        }

        isFolded = true;
    }

    public void Unfold()
    {
        int count = cards.Count;
        if (count == 0) return;

        float angleStep = useFixedAngleStep ? fixedAngleStep : (count == 1 ? 0f : maxAngle / (count - 1));
        float startAngle = useFixedAngleStep
            ? -angleStep * (count - 1) / 2f
            : -maxAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));

            Vector2 targetPos = dir * radius;
            targetPos.x *= spacingMultiplier;

            Quaternion targetRot = Quaternion.Euler(0f, 0f, faceCenter ? -angle : 0f);
            float delay = fanoutDelay + (useSequentialDelay ? i * perCardDelay : 0f);

            var card = cards[i];
            var seq = DOTween.Sequence();

            if (useScaleAnimation)
                card.localScale = startScale;

            seq.AppendInterval(delay);
            seq.AppendCallback(() => PlayAnimation(card, unfoldStartClipName));
            seq.AppendInterval(0.3f);

            seq.Join(card.DOAnchorPos(targetPos, animationDuration).SetEase(positionEase));
            seq.Join(card.DOLocalRotateQuaternion(targetRot, animationDuration).SetEase(rotationEase));
            if (useScaleAnimation)
                seq.Join(card.DOScale(endScale, animationDuration).SetEase(scaleEase));

            seq.OnComplete(() => OnCardUnfoldComplete(card));
            LockChildLocalTransform(card);
        }

        isFolded = false;
    }

    public void Fold()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            float delay = useSequentialDelay ? i * perCardDelay : 0f;
            var card = cards[i];

            card.DOAnchorPos(foldAnchoredPosition, animationDuration)
                .SetDelay(delay).SetEase(positionEase)
                .OnComplete(() => OnCardFoldComplete(card));

            Quaternion targetRot = Quaternion.Euler(foldEulerRotation);
            card.DOLocalRotateQuaternion(targetRot, animationDuration)
                .SetDelay(delay).SetEase(rotationEase);

            if (useScaleAnimation)
                card.DOScale(startScale, animationDuration)
                    .SetDelay(delay).SetEase(scaleEase);

            LockChildLocalTransform(card);
        }

        isFolded = true;
    }

    public void Toggle()
    {
        if (isFolded) Unfold();
        else Fold();
    }

    void PlayAnimation(RectTransform card, string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        Animation anim = card.GetComponent<Animation>();
        if (anim != null && anim.GetClip(clipName) != null)
            anim.Play(clipName);
    }

    void OnCardUnfoldComplete(RectTransform card)
    {
        PlayAnimation(card, unfoldEndClipName);
    }

    void OnCardFoldComplete(RectTransform card)
    {
        PlayAnimation(card, foldEndClipName);
    }

    void LockChildLocalTransform(RectTransform card)
    {
        foreach (Transform child in card)
        {
            child.localRotation = Quaternion.identity;
            child.localPosition = Vector3.zero;
        }
    }
}
