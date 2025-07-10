using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTextCycler : MonoBehaviour
{
    [Header("기본 설정")]
    public TextMeshProUGUI targetText;
    [TextArea] public string[] messages;

    [Header("시간 설정")]
    public float changeInterval = 2f;
    public float preDelay = 0.5f;
    public float typingSpeed = 0.05f;

    [Header("옵션 설정")]
    public bool loop = true;
    public bool useTypingEffect = false;
    public bool randomize = false;
    public bool noRepeatRandom = false;

    private List<string> messageQueue = new List<string>();
    private int currentIndex = 0;
    private Coroutine cycleRoutine;

    void Awake()
    {
        // targetText 자동 연결
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }
    }

    void OnEnable()
    {
        ResetAndStart();
    }

    void OnDisable()
    {
        if (cycleRoutine != null)
            StopCoroutine(cycleRoutine);
    }

    public void ResetAndStart()
    {
        if (cycleRoutine != null)
            StopCoroutine(cycleRoutine);

        BuildMessageQueue();
        currentIndex = 0;
        cycleRoutine = StartCoroutine(TextCycleRoutine());
    }

    void BuildMessageQueue()
    {
        messageQueue.Clear();

        if (noRepeatRandom)
        {
            messageQueue.AddRange(messages);
            Shuffle(messageQueue);
        }
        else if (randomize)
        {
            messageQueue.AddRange(messages);
        }
        else
        {
            messageQueue.AddRange(messages);
        }
    }

    IEnumerator TextCycleRoutine()
    {
        while (true)
        {
            if (messageQueue.Count == 0)
                yield break;

            if (preDelay > 0f)
            {
                targetText.text = "";
                yield return new WaitForSeconds(preDelay);
            }

            string message;
            if (noRepeatRandom || !randomize)
            {
                message = messageQueue[currentIndex];
                currentIndex++;
            }
            else
            {
                message = messages[Random.Range(0, messages.Length)];
            }

            if (useTypingEffect)
            {
                yield return StartCoroutine(TypeText(message));
            }
            else
            {
                targetText.text = message;
            }

            if ((noRepeatRandom || !randomize) && currentIndex >= messageQueue.Count)
            {
                if (loop)
                {
                    BuildMessageQueue();
                    currentIndex = 0;
                }
                else
                {
                    yield break;
                }
            }

            yield return new WaitForSeconds(changeInterval);
        }
    }

    IEnumerator TypeText(string message)
    {
        targetText.text = "";
        foreach (char c in message)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
