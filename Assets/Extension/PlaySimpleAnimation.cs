using UnityEngine;

public class PlaySimpleAnimation : MonoBehaviour
{
    public Animation animationComponent; // Animation ????
    public AnimationClip animationClip;  // ?????? ??? ????? ??

    public void PlayAnim()
    {
        if (animationComponent != null && animationClip != null)
        {
            animationComponent.Play(animationClip.name);
        }
    }
}
