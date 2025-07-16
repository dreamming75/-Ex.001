using TMPro;
using UnityEngine;
using System.Collections;

public class TMPNumberCounter : MonoBehaviour
{
    [Header("필수: 연결할 TMP 텍스트")]
    public TextMeshProUGUI tmpText;

    [Header("카운팅 설정")]
    [SerializeField] int _startValue = 0;
    [SerializeField] int _endValue = 100;
    public bool useEndValue = true;
    public float duration = 1.0f;
    public float endValueApplyDelay = 0.05f;
    public bool playOnEnable = true;
    public bool useCommaFormat = false;

    [Header("외부 EndValue 도착시에만 카운트 시작")]
    public bool countOnlyWhenExternalEndValue = false;

    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useScaleAnimation = true;
    public Vector3 scaleFrom = Vector3.one;
    public Vector3 scaleTo = new Vector3(1.3f, 1.3f, 1f);
    public float scaleDuration = 0.15f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Coroutine countCoroutine;
    Coroutine scaleCoroutine;
    Coroutine playOnEnableCoroutine;

    private bool isCounting = false;

    private int? pendingExternalEndValue = null;

    public int startValue
    {
        get => _startValue;
        set
        {
            _startValue = value;
            TryStartCountingFromExternal();
        }
    }

    public int endValue
    {
        get => _endValue;
        set
        {
            _endValue = value;
            TryStartCountingFromExternal();
        }
    }

    /// <summary>
    /// 외부에서 값을 받아올 때 endValue 프로퍼티에도 즉시 반영
    /// </summary>
    public void SetEndValueFromExternal(int externalValue)
    {
        endValue = externalValue;

        if (isActiveAndEnabled)
        {
            pendingExternalEndValue = null;
            StartCounting(startValue, endValue);
        }
        else
        {
            // Enable될 때 카운트 시작하도록 값 저장
            pendingExternalEndValue = externalValue;
        }
    }

    private void OnEnable()
    {
        isCounting = false;

        if (tmpText == null)
        {
            Debug.LogWarning("TMP 텍스트가 연결되지 않았습니다!");
            return;
        }

        tmpText.text = FormatNumber(startValue);
        tmpText.rectTransform.localScale = scaleFrom;

        if (countOnlyWhenExternalEndValue)
        {
            // 외부에서 EndValue가 들어온 적이 있으면 카운트 시작
            if (pendingExternalEndValue.HasValue)
            {
                int value = pendingExternalEndValue.Value;
                pendingExternalEndValue = null;
                StartCounting(startValue, value);
            }
            // 아니면 대기
            return;
        }

        if (playOnEnable)
        {
            if (playOnEnableCoroutine != null)
                StopCoroutine(playOnEnableCoroutine);

            playOnEnableCoroutine = StartCoroutine(PlayOnEnableDelayRoutine());
        }
    }

    private IEnumerator PlayOnEnableDelayRoutine()
    {
        yield return new WaitForSeconds(endValueApplyDelay);

        if (!isCounting)
        {
            int actualEndValue = useEndValue
                ? endValue
                : ParseCurrentTMPValueOrDefault(endValue);

            StartCounting(startValue, actualEndValue);
        }
    }

    private void TryStartCountingFromExternal()
    {
        if (!isActiveAndEnabled || isCounting)
            return;

        if (pendingExternalEndValue.HasValue)
        {
            endValue = pendingExternalEndValue.Value;
            StartCounting(startValue, endValue);
            pendingExternalEndValue = null;
            return;
        }

        if (useEndValue)
        {
            StartCounting(startValue, endValue);
        }
        else
        {
            StartCounting(startValue, ParseCurrentTMPValueOrDefault(endValue));
        }
    }

    public void StartCounting(int from, int to)
    {
        if (tmpText == null) return;

        if (countCoroutine != null)
            StopCoroutine(countCoroutine);

        countCoroutine = StartCoroutine(CountRoutine(from, to));
        isCounting = true;
    }

    private IEnumerator CountRoutine(int from, int to)
    {
        float elapsed = 0f;
        int lastValue = from;
        isCounting = true;

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

        if (useScaleAnimation && to != lastValue)
            PlayScaleAnimation();

        isCounting = false;
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
        while (time < scaleDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / scaleDuration);
            float evalT = scaleCurve.Evaluate(t);
            rect.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, evalT);
            yield return null;
        }
        time = 0f;
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
        int actualEndValue = useEndValue
            ? endValue
            : ParseCurrentTMPValueOrDefault(endValue);
        StartCounting(startValue, actualEndValue);
    }

    private int ParseCurrentTMPValueOrDefault(int fallback)
    {
        if (tmpText == null) return fallback;
        string raw = tmpText.text.Replace(",", "");
        int v;
        if (int.TryParse(raw, out v))
            return v;
        return fallback;
    }
}
