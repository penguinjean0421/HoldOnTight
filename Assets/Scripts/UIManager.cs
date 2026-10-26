using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text Fields")]
    public Text scoreText;
    public Text timerText;
    public Text linesText;
    public Image stageIconImage;

    [Header("UI Panels")]
    public GameObject gameStart;
    public GameObject selectStage;
    public GameObject inGame;
    public GameObject gameOver;
    public GameObject stageClearPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetUI();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    public void UpdateTimerUI(float time, bool isTimeAttack)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        string label = isTimeAttack ? "LEFT" : "TIME";
        timerText.text = $"{label}: {minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    public void UpdateLinesUI(int current, int target)
    {
        if (linesText != null) linesText.text = $"LINES: {current} / {target}";
    }

    public void UpdateStageIcon(Sprite sprite)
    {
        stageIconImage.sprite = sprite;
        stageIconImage.gameObject.SetActive(sprite != null);
    }

    public void ShowGameOver()
    {
        if (gameOver != null) gameOver.SetActive(true);
    }

    public void ShowStageClear()
    {
        if (stageClearPanel != null) stageClearPanel.SetActive(true);
    }

    public void ResetUI()
    {
        UpdateScore(0);

        if (StageManager.Instance != null)
        {
            UpdateTimerUI(StageManager.Instance.stageTimer, StageManager.Instance.isTimeAttack);
            UpdateLinesUI(0, StageManager.Instance.targetLines);
        }

        SetPanelActive(gameStart, true);
        SetPanelActive(selectStage, false);
        SetPanelActive(inGame, false);
        SetPanelActive(gameOver, false);
        SetPanelActive(stageClearPanel, false);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    // 단 하나의 패널만 활성화하고 나머지는 비활성화하는 헬퍼 메서드
    private void ShowPanel(GameObject targetPanel)
    {
        SetPanelActive(gameStart, targetPanel == gameStart);
        SetPanelActive(selectStage, targetPanel == selectStage);
        SetPanelActive(inGame, targetPanel == inGame);
        SetPanelActive(gameOver, targetPanel == gameOver);
        SetPanelActive(stageClearPanel, targetPanel == stageClearPanel);
    }

    void ResumeTime()
    {
        Time.timeScale = 1f;
    }

    // 1. 시작하기 (타이틀 화면 -> 스테이지 선택창)
    public void GameStart()
    {
        GoToStageSelect();
    }

    public void PlayGame()
    {
        ResumeTime();
        ShowPanel(inGame);
    }

    public void GoToTitle()
    {
        ResumeTime();
        ClearBoardTiles();

        ResetUI();

        AudioManager.Instance.PlayTitleBGM();

    }

    public void GoToStageSelect()
    {
        ResumeTime();
        ClearBoardTiles();
        ShowPanel(selectStage);

        AudioManager.Instance.PlayTitleBGM();
    }

    public void ReStart()
    {
        ResumeTime();
        ShowPanel(inGame);

        GameManager.Instance.ReStart();
    }

    private void ClearBoardTiles()
    {
        if (GameManager.Instance != null && GameManager.Instance.board != null)
        {
            Board board = GameManager.Instance.board;
            if (board.tilemap != null) board.tilemap.ClearAllTiles();
            if (board.previewTilemap != null) board.previewTilemap.ClearAllTiles();
            if (board.holdTilemap != null) board.holdTilemap.ClearAllTiles();
        }
    }
}