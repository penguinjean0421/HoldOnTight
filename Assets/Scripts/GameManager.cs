using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Board board;
    public Piece piece;

    public int score { get; private set; }
    public bool isGameOver { get; private set; }

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
        NewGame();
    }

    public void NewGame()
    {
        isGameOver = false;
        score = 0;
        Time.timeScale = 1f;

        StageManager.Instance.ResetStage();
        UIManager.Instance.ResetUI();

        if (UIManager.Instance.inGame.activeInHierarchy) { AudioManager.Instance.PlayGameplayBGM(); }
        else { AudioManager.Instance.PlayTitleBGM(); }

        if (board != null) board.ResetBoard();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (UIManager.Instance != null) UIManager.Instance.UpdateScore(score);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        isGameOver = true;

        if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
    }

    #region Button UI Func
    public void ReStart()
    {
        NewGame();
    }
    #endregion
}