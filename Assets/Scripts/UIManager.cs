using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text Fields")]
    public Text scoreText;
    public Text timerText;
    public Text tSpinCountText;

    [Header("UI Panels")]
    public GameObject gameOver; // (선택) 게임오버 창
    public GameObject missionClear;

    void Awake()
    {
        // 싱글톤 인스턴스
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        ResetUI();
    }

    // 점수 UI 갱신
    public void UpdateScore(int score)
    {
        if (scoreText != null) { scoreText.text = $"Score: {score}"; }
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public void UpdateTSpinCount(int current, int target)
    {
        if (tSpinCountText != null)
        {
            tSpinCountText.text = $"T-Spin: {current} / {target}";
        }
    }

    // 게임 오버 처리 UI
    public void ShowGameOver()
    {
        if (gameOver != null) { gameOver.SetActive(true); }
    }

    public void ShowMissionClear()
    {
        if (missionClear != null) { missionClear.SetActive(true); }
    }

    // UI 초기화
    public void ResetUI()
    {
        UpdateScore(0);

        if (gameOver != null) { gameOver.SetActive(false); }
    }

    #region Buttons
    public void ReStart()
    {
        // 게임오버 씬 끄고,
        gameOver.SetActive(false);

        // 게임매니저에서 리스타트 로직 소환하기
        GameManager.Instance.ReStart();
    }
    #endregion
}