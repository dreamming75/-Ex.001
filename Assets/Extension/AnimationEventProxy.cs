using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    public ScoreDisplay scoreTarget; // 점수 보여주는 쪽

    public void NotifyScoreReady()
    {
        if (scoreTarget != null)
        {
            scoreTarget.OnExternalAnimationEnd();
        }
    }
}
