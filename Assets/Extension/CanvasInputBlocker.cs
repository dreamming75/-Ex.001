using UnityEngine;

public class CanvasInputBlocker : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float blockTime = 2f;
    
    [Header("버튼의 interactable 활성화 여부")]
    public bool blockInputOnly = true; // true: 입력만 막기, false: 아예 비활성처럼

    void OnEnable()
    {
        StartCoroutine(BlockInput());
    }

    System.Collections.IEnumerator BlockInput()
    {
        if (blockInputOnly)
        {
            // 입력(클릭 등)만 막고, 비주얼/상호작용은 그대로
            canvasGroup.blocksRaycasts = false;
            // interactable은 손대지 않음
        }
        else
        {
            // 아예 버튼도 비활성처럼(눌러지지도 않음)
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        yield return new WaitForSeconds(blockTime);

        // 원래대로 복구
        canvasGroup.blocksRaycasts = true;
        if (!blockInputOnly)
            canvasGroup.interactable = true;
    }
}
