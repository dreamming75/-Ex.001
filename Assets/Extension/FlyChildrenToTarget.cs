//자식 오브젝트들이 타겟으로 날아가기
//생성좌표 0 -> 램덤위치
//날아가기 전에 대기 시간
//날아가는 도중 X, Y 오프셋 랜덤 추가
//날아가는 시간은 범위 내 랜덤
//도착 후 애니메이션 재생


using UnityEngine;
using System.Collections;

public class FlyChildrenToTarget : MonoBehaviour
{
    [Header("자식 오브젝트 설정")]
    public GameObject childPrefab;
    public int childCount = 3;
    public float childSpawnDuration = 2f;

    [Header("타겟 및 타이밍 설정")]
    public Transform target;
    public Vector2 flyDurationRange = new Vector2(1f, 3f);

    [Header("커브 경로 설정")]
    public Vector3 controlPointOffset = new Vector3(0f, 2f, 0f);
    public Vector2 randomXRange = new Vector2(-1f, 1f);
    public Vector2 randomYRange = new Vector2(-1f, 1f);

    [Header("비행 전 대기 시간")]
    public float minWaitBeforeFly = 0.5f;
    public float maxWaitBeforeFly = 2f;

    [Header("랜덤 생성 위치 설정")]
    public Vector2 spawnRangeX = new Vector2(-5f, 5f);
    public Vector2 spawnRangeY = new Vector2(0f, 5f);
    public Vector2 spawnRangeZ = new Vector2(-5f, 5f);

    [Header("랜덤 회전 설정")]
    public Vector2 rotationRangeX = new Vector2(0f, 360f);
    public Vector2 rotationRangeY = new Vector2(0f, 360f);
    public Vector2 rotationRangeZ = new Vector2(0f, 360f);

    [Header("랜덤 크기 설정")]
    public float minScale = 0.5f;
    public float maxScale = 2f;

    [Header("초기 이동 설정")]
    public float moveToRandomDuration = 0.5f;
    public AnimationCurve moveEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("생성 위치 옵션")]
    public bool spawnAtOrigin = true;

    void OnEnable()
    {
        StartCoroutine(SpawnChildren());
    }

    IEnumerator SpawnChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float[] spawnTimes = new float[childCount];
        for (int i = 0; i < childCount; i++)
        {
            spawnTimes[i] = Random.Range(0f, childSpawnDuration);
        }
        System.Array.Sort(spawnTimes);

        float currentTime = 0f;

        for (int i = 0; i < childCount; i++)
        {
            float waitTime = spawnTimes[i] - currentTime;
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            currentTime = spawnTimes[i];

            GameObject child = Instantiate(childPrefab, transform);

            Vector3 initialPos = spawnAtOrigin ? Vector3.zero : GetRandomSpawnPosition();
            child.transform.localPosition = initialPos;
            child.transform.localRotation = GetRandomRotation();
            child.transform.localScale = GetRandomScale();

            if (spawnAtOrigin)
            {
                Vector3 targetRandomPosition = GetRandomSpawnPosition();
                StartCoroutine(MoveToRandomPosition(child.transform, targetRandomPosition, moveToRandomDuration));
            }

            float waitBeforeFly = Random.Range(minWaitBeforeFly, maxWaitBeforeFly);
            StartCoroutine(WaitAndFly(child.transform, waitBeforeFly));
        }
    }

    IEnumerator MoveToRandomPosition(Transform obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = moveEaseCurve.Evaluate(t);
            obj.localPosition = Vector3.Lerp(startPos, targetPos, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.localPosition = targetPos;
    }

    IEnumerator WaitAndFly(Transform child, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(FlyToTargetWithCurve(child));
    }

    IEnumerator FlyToTargetWithCurve(Transform child)
    {
        Vector3 startPos = child.position;
        Vector3 endPos = target.position;

        float randomX = Random.Range(randomXRange.x, randomXRange.y);
        float randomY = Random.Range(randomYRange.x, randomYRange.y);
        Vector3 controlPoint = startPos + controlPointOffset + new Vector3(randomX, randomY, 0f);

        float duration = Random.Range(flyDurationRange.x, flyDurationRange.y);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 curvePos = Mathf.Pow(1 - t, 2) * startPos + 2 * (1 - t) * t * controlPoint + Mathf.Pow(t, 2) * endPos;
            child.position = curvePos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        child.position = endPos;

        Animator animator = child.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("OnArrived");
        }
    }

    Quaternion GetRandomRotation()
    {
        float x = Random.Range(rotationRangeX.x, rotationRangeX.y);
        float y = Random.Range(rotationRangeY.x, rotationRangeY.y);
        float z = Random.Range(rotationRangeZ.x, rotationRangeZ.y);
        return Quaternion.Euler(x, y, z);
    }

    Vector3 GetRandomScale()
    {
        float s = Random.Range(minScale, maxScale);
        return new Vector3(s, s, s);
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(spawnRangeX.x, spawnRangeX.y);
        float y = Random.Range(spawnRangeY.x, spawnRangeY.y);
        float z = Random.Range(spawnRangeZ.x, spawnRangeZ.y);
        return new Vector3(x, y, z);
    }
}
