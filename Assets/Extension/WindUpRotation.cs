using System.Collections;
using UnityEngine;

public class WindUpRotation : MonoBehaviour
{
    [Header("회전 설정")]
    public float totalRotationTime = 2f;     // 360도 도는 데 걸리는 시간
    public float rotationStep = 30f;         // 한 번에 돌릴 각도
    public float waitTime = 0.5f;            // 회전 후 멈추는 시간
    public bool loop = true;                 // 루프 여부
    public bool clockwise = true;            // 정방향 / 역방향

    [Header("딜레이 설정")]
    public float waitBeforeStart = 0f;       // 오브젝트가 켜지고 회전 시작까지의 대기 시간

    private RectTransform rectTransform;
    private Transform cachedTransform;
    private Coroutine rotateCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cachedTransform = transform;
    }

    void Start()
    {
        StartRotationFromBeginning();
    }

    void OnEnable()
    {
        StartRotationFromBeginning();
    }

    void OnDisable()
    {
        StopRotation();
    }

    private void StartRotationFromBeginning()
    {
        StopRotation();
        ResetRotation();
        rotateCoroutine = StartCoroutine(StartWithDelayAndRotate());
    }

    private void StopRotation()
    {
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }
    }

    private void ResetRotation()
    {
        SetZRotation(0f);
    }

    private IEnumerator StartWithDelayAndRotate()
    {
        if (waitBeforeStart > 0f)
            yield return new WaitForSeconds(waitBeforeStart);

        yield return RotateZ();
    }

    private IEnumerator RotateZ()
    {
        float speed = 360f / totalRotationTime;
        float duration = rotationStep / speed;

        while (true)
        {
            float startZ = GetZRotation();
            float endZ = startZ + (clockwise ? rotationStep : -rotationStep);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float z = Mathf.Lerp(startZ, endZ, t);
                SetZRotation(z);
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetZRotation(endZ);
            yield return new WaitForSeconds(waitTime);

            if (!loop) break;
        }
    }

    private float GetZRotation()
    {
        if (rectTransform != null)
            return rectTransform.localEulerAngles.z;
        return cachedTransform.localEulerAngles.z;
    }

    private void SetZRotation(float z)
    {
        Vector3 newRotation = new Vector3(0f, 0f, z);

        if (rectTransform != null)
            rectTransform.localEulerAngles = newRotation;
        else
            cachedTransform.localEulerAngles = newRotation;
    }
}
