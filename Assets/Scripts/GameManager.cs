using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Board board;
    public Piece piece;

    public int score;

    [Header("Gravity Acceleration (중력 기울기)")]
    public float initialStepDelay = 1.0f;
    public float minStepDelay = 0.05f;
    public float speedUpRate = 0.015f;

    [Header("T-Spin Time Attack Mission")]
    public float timeLimit = 180f;
    public int targetTSpinCount = 5;
    public float remainingTime { get; private set; }
    public int currentTSpinCount { get; private set; }
    public bool isGameActive { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        NewGame();
    }

    void Update()
    {
        if (!isGameActive) { return; }

        // 타임 어택 타이머 연산
        remainingTime -= Time.deltaTime;
        UIManager.Instance.UpdateTimer(remainingTime);

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            GameOver(); // 제한 시간 초과시 게임 오버
        }
    }


    void NewGame()
    {
        score = 0;
        remainingTime = timeLimit;
        currentTSpinCount = 0;
        isGameActive = true;
        Time.timeScale = 1f;

        UIManager.Instance.ResetUI();
        UIManager.Instance.UpdateTSpinCount(currentTSpinCount, targetTSpinCount);
    }

    // 중력 기울기에 따른 stepDelay 계산
    public float GetCurrentStepDelay()
    {
        float elapsedTime = timeLimit - remainingTime;
        float delay = initialStepDelay - (elapsedTime * speedUpRate);
        return Mathf.Max(minStepDelay, delay);
    }

    // T-Spin 성공 시 호출
    public void RegisterTSpin()
    {
        if (!isGameActive) return;

        currentTSpinCount++;
        UIManager.Instance.UpdateTSpinCount(currentTSpinCount, targetTSpinCount);

        // 미션 성공 조건 충족 시
        if (currentTSpinCount >= targetTSpinCount)
        {
            MissionClear();
        }
    }

    public void MissionClear()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        UIManager.Instance.ShowMissionClear();
    }

    public void AddScore(int amount)
    {
        score += amount;

        UIManager.Instance.UpdateScore(score);
    }

    // 게임 오버 처리
    public void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOver();
    }


    #region Button UI Func

    public void ReStart()
    {
        board.tilemap.ClearAllTiles();

        if (board.previewTilemap != null) { board.previewTilemap.ClearAllTiles(); }
        if (board.holdTilemap != null) { board.holdTilemap.ClearAllTiles(); }

        NewGame();
    }

    #endregion

}
