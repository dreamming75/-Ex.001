//숫자 카운팅 (커브 기반 속도 조절)
//카운팅할 때마다 스케일 애니메이션
//스케일 범위 및 지속 시간 지정 가능
//스케일 애니메이션도 커브로 부드럽게
//쉼표 포맷 옵션
//오브젝트 켜질 때 자동 실행
//마지막 숫자에서 중복 애니메이션 제거




using TMPro;
using UnityEngine;
using System.Collections;

public class TMPNumberCounter : MonoBehaviour
{
    [Header("필수: 연결할 TMP 텍스트")]
    public TextMeshProUGUI tmpText;

    [Header("카운팅 설정")]
    public int startValue = 0;
    public int endValue = 100;
    public float duration = 1.0f;

    [Header("카운팅 속도 커브")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("애니메이션 옵션")]
    public bool playOnEnable = true;
    public bool useCommaFormat = false;

    [Header("스케일 애니메이션 옵션")]
    public bool useScaleAnimation = true;
    public Vector3 scaleFrom = Vector3.one;
    public Vector3 scaleTo = new Vector3(1.3f, 1.3f, 1f);
    public float scaleDuration = 0.15f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine countCoroutine;
    private Coroutine scaleCoroutine;

    void OnEnable()
    {
        if (tmpText == null)
        {
            Debug.LogWarning("TMP 텍스트가 연결되지 않았습니다!");
            return;
        }

        tmpText.text = FormatNumber(startValue);
        tmpText.rectTransform.localScale = scaleFrom;

        if (playOnEnable)
        {
            StartCounting(startValue, endValue);
        }
    }

    public void StartCounting(int from, int to)
    {
        if (tmpText == null) return;

        if (countCoroutine != null)
            StopCoroutine(countCoroutine);

        countCoroutine = StartCoroutine(CountRoutine(from, to));
    }

    private IEnumerator CountRoutine(int from, int to)
    {
        float elapsed = 0f;
        int lastValue = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = curve.Evaluate(t);
            int current = Mathf.RoundToInt(Mathf.Lerp(from, to, curvedT));

            if (current != lastValue)
            {
                tmpText.text = FormatNumber(current);
                lastValue = current;

                if (useScaleAnimation)
                    PlayScaleAnimation();
            }

            yield return null;
        }

        tmpText.text = FormatNumber(to);

        // 마지막 숫자가 이미 표시되어있지 않았다면, 애니메이션 한 번 실행
        if (useScaleAnimation && to != lastValue)
        {
            PlayScaleAnimation();
        }
    }

    private void PlayScaleAnimation()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleAnimationCoroutine());
    }

    private IEnumerator ScaleAnimationCoroutine()
    {
        RectTransform rect = tmpText.rectTransform;

        float time = 0f;
        // 커지는 구간
        while (time < scaleDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / scaleDuration);
            float evalT = scaleCurve.Evaluate(t);
            rect.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, evalT);
            yield return null;
        }

        time = 0f;
        // 줄어드는 구간
        while (time < scaleDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / scaleDuration);
            float evalT = scaleCurve.Evaluate(t);
            rect.localScale = Vector3.LerpUnclamped(scaleTo, scaleFrom, evalT);
            yield return null;
        }

        rect.localScale = scaleFrom;
    }

    private string FormatNumber(int number)
    {
        return useCommaFormat ? number.ToString("N0") : number.ToString();
    }

    [ContextMenu("🔁 인스펙터에서 수동 실행")]
    public void RunCountManually()
    {
        StartCounting(startValue, endValue);
    }
}
