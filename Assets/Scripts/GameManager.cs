using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Board board;
    public Piece piece;

    public int score;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        NewGame();
    }

    void NewGame()
    {
        score = 0;
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
        // Time.timeScale = 0f;

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
