using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Board board;
    public Piece piece;

    public int score;

    [Header("Earthquake Event")]
    public Camera mainCamera;
    public float minEarthquakeInterval = 15f; // 지진 최소 주기(초)
    public float maxEarthquakeInterval = 30f; // 지진 최대 주기(초)
    public float hardDropSealDuration = 5f;    // 봉인 유지 시간
    float nextEarthquakeTime;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (mainCamera == null) { mainCamera = Camera.main; }
        ScheduleNextEarthquake();
        NewGame();
    }
    void Update()
    {
        if (Time.time >= nextEarthquakeTime)
        {
            TriggerEarthquake();
            ScheduleNextEarthquake();
        }
    }

    void ScheduleNextEarthquake()
    {
        nextEarthquakeTime = Time.time + Random.Range(minEarthquakeInterval, maxEarthquakeInterval);
    }

    [ContextMenu("Trigger Earthquake")]
    public void TriggerEarthquake()
    {
        // 1. 카메라 흔들림 연출
        if (mainCamera != null) { StartCoroutine(ShakeCamera(0.6f, 0.25f)); }

        // 2. 하드 드롭 봉인 기막 적용
        if (piece != null)
        {
            piece.SealHardDrop(hardDropSealDuration);
        }
    }

    System.Collections.IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    void NewGame()
    {
        score = 0;
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
