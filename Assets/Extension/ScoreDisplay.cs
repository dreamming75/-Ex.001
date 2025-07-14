using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;  // 버튼 연결에 필요

public class ScoreDisplay : MonoBehaviour
{
    public TMP_Text scoreText;
    public int minScore = 100;
    public int maxScore = 1000;
    private bool scoreShown = false;

    // 🔹 버튼 연결
    public Button resetButton;

    void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetScore);
    }

    // 외부에서 애니메이션이 끝났다고 알림
    public void OnExternalAnimationEnd()
    {
        if (!scoreShown)
        {
            ShowRandomScore();
            scoreShown = true;
        }
    }

    void ShowRandomScore()
    {
        int score = Random.Range(minScore, maxScore + 1);
        scoreText.text = score.ToString();
        scoreText.alpha = 0f;
        StartCoroutine(FadeInScore());
    }

    IEnumerator FadeInScore()
    {
        float duration = 0.2f;
        float timer = 0.1f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);
            scoreText.alpha = alpha;
            timer += Time.deltaTime;
            yield return null;
        }

        scoreText.alpha = 1f;
    }

    // 🔸 버튼 클릭 시 초기화
    public void ResetScore()
    {
        scoreText.text = "";
        scoreText.alpha = 0.5f;     // 다시 보여줄 준비
        scoreShown = false;
    }
}
