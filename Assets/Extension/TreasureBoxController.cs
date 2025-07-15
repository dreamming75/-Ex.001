using UnityEngine;

public class TreasureBoxController : MonoBehaviour
{
    [Header("애니메이션")]
    public AnimationClip shakeClip;

    [Header("설정")]
    public int minClicks = 3;
    public int maxClicks = 6;
    public int minGold = 1000;
    public int maxGold = 50000;

    private int requiredClicks;
    private int currentClicks;
    private bool isOpened;

    private Animation anim;
    private GameController gameController;

    // 초기화 (클릭 카운터 및 애니메이션 설정)
    public void Init(GameController controller)
    {
        gameController = controller;
        currentClicks = 0;
        requiredClicks = Random.Range(minClicks, maxClicks + 1);
        isOpened = false;

        // Animation 컴포넌트 세팅
        anim = GetComponent<Animation>() ?? gameObject.AddComponent<Animation>();
        if (shakeClip != null && anim.GetClip(shakeClip.name) == null)
            anim.AddClip(shakeClip, shakeClip.name);
    }

    // 클릭 처리 (설정된 횟수 도달 시 GameController에 알림)
    public void OnBoxClicked()
    {
        if (isOpened) return;

        currentClicks++;
        if (shakeClip != null)
            anim.Play(shakeClip.name);

        if (currentClicks >= requiredClicks)
        {
            isOpened = true;
            int gold = Random.Range(minGold, maxGold + 1);
            gameController.OnBoxOpened(gold);
        }
    }
}
