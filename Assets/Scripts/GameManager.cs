using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Board board;
    public Piece piece;

    public int score;

    [Header("Time Attack Settings")]
    public bool isTimeAttack = true;
    public float timeLimit = 180f;
    public float remainingTime { get; private set; }
    public float gameTime { get; private set; }
    private bool isGameOver = false;

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
        if (isGameOver) { return; }

        gameTime += Time.deltaTime;

        if (isTimeAttack)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                GameOver(); // 제한시간 초과 시 게임오버
            }
            UIManager.Instance.UpdateTimer(remainingTime);
        }
    }

    void NewGame()
    {
        score = 0;
        gameTime = 0f;
        remainingTime = timeLimit;
        isGameOver = false;
        Time.timeScale = 1f;

        UIManager.Instance.ResetUI();
    }

    public void AddScore(int amount)
    {
        score += amount;

        UIManager.Instance.UpdateScore(score);
    }

    // 게임 오버 처리
    public void GameOver()
    {
        isGameOver = true;
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
