// 오브젝트가 일정간격으로 램덤으로 생성


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSequentialActivator : MonoBehaviour
{
    public float interval = 1.0f; // 오브젝트 간 간격
    public bool loop = false;     // 무한 반복 여부

    private List<Transform> childList = new List<Transform>();
    private Coroutine activationRoutine;

    void Awake()
    {
        // 자식 목록 미리 수집
        childList.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            childList.Add(child);
        }
    }

    void OnEnable()
    {
        // 모든 자식 비활성화
        foreach (var child in childList)
            child.gameObject.SetActive(false);

        // 기존 루틴 중지
        if (activationRoutine != null)
            StopCoroutine(activationRoutine);

        activationRoutine = StartCoroutine(ActivateChildrenInRandomOrder());
    }

    void OnDisable()
    {
        if (activationRoutine != null)
        {
            StopCoroutine(activationRoutine);
            activationRoutine = null;
        }
    }

    IEnumerator ActivateChildrenInRandomOrder()
    {
        do
        {
            Shuffle(childList);

            foreach (var child in childList)
            {
                child.gameObject.SetActive(true);
                yield return new WaitForSeconds(interval);
            }

        } while (loop);
    }

    void Shuffle(List<Transform> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
