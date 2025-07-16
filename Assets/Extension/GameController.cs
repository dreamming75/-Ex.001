using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Scene에 배치된 루트 Box 오브젝트")]
    public GameObject box; // Fx_RewardBox_Open 혹은 Box 전체를 가리키도록 설정

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public Button boxButton;
    public Button replayButton;

    private TreasureBoxController controller;

    public TMPNumberCounter scoreCounter;

    void Start()
    {
        // 필수 참조 체크
        if (box == null)
        {
            Debug.LogError("GameController: 'box' GameObject가 할당되지 않았습니다.");
            return;
        }
        // TreasureBoxController는 'Box' 하위에 있을 수 있으므로 GetComponentInChildren 사용
        controller = box.GetComponentInChildren<TreasureBoxController>();
        if (controller == null)
        {
            Debug.LogError("GameController: TreasureBoxController를 찾을 수 없습니다.");
            return;
        }

        // 초기화
        controller.Init(this);
        ToggleOpenClose(false);

        if (scoreCounter != null)
            scoreCounter.gameObject.SetActive(false); // 처음엔 꺼둠
        boxButton.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(false);

        boxButton.onClick.AddListener(OnBoxClicked);
        replayButton.onClick.AddListener(OnReplay);
    }

    public void OnBoxClicked()
    {
        controller?.OnBoxClicked();
    }

    public void OnBoxOpened(int gold)
    {
        ToggleOpenClose(true);
        if (scoreCounter != null)
        {
            scoreCounter.gameObject.SetActive(true); // TMPNumberCounter가 붙은 오브젝트 활성화
            scoreCounter.SetEndValueFromExternal(gold); // 카운트 애니메이션 시작
        }
        boxButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(true);
    }

    public void OnReplay()
    {
        controller.Init(this);
        ToggleOpenClose(false);
        if (scoreCounter != null)
            scoreCounter.gameObject.SetActive(false); // 다시 끔
        boxButton.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(false);
    }

    private void ToggleOpenClose(bool open)
    {
        // container 선택: box 자체 또는 그 하위 'Box' 객체
        Transform container = box.transform;
        Transform nested = box.transform.Find("Box");
        if (nested != null)
            container = nested;

        Transform openT = container.Find("Box_Open");
        Transform closeT = container.Find("Box_Close");
        if (openT == null || closeT == null)
        {
            Debug.LogError($"GameController: Box_Open 또는 Box_Close를 찾을 수 없습니다. (컨테이너: {container.name})");
            return;
        }

        openT.gameObject.SetActive(open);
        closeT.gameObject.SetActive(!open);
    }
}
