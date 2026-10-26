using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Gimmick Toggles")]
    public bool sealIBlock = false;
    public bool variableDropSpeed = false;
    public bool isTimeAttack = false;
    public bool infectedBlock = false;
    public bool earthquake = false;
    public bool disableHardDrop = false;

    [Header("Stage Rules")]
    public int targetLines = 40;
    public float timeAttackLimit = 60f;

    [Header("Gimmick Settings")]
    public float minDropDelay = 0.15f;
    public float maxDropDelay = 0.8f;
    public float infectionInterval = 6f;
    public Tile infectedTile;
    public float earthquakeInterval = 10f;

    [Header("Stage Status")]
    public float stageTimer { get; private set; }
    public int clearedLines { get; private set; }
    public bool isStageCleared { get; private set; }

    private float infectionTimer;
    private float earthquakeTimer;
    private Vector3 originalCameraPos;
    private Camera mainCamera;

    // 감염 블록 좌표 탐색용 버퍼 (GC Alloc 예방)
    private readonly List<Vector3Int> occupiedPositionsBuffer = new List<Vector3Int>(200);

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
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.position;
        }
    }

    public void ApplyStageData(StageData data)
    {
        sealIBlock = data.sealIBlock;
        variableDropSpeed = data.variableDropSpeed;
        isTimeAttack = data.isTimeAttack;
        infectedBlock = data.infectedBlock;
        earthquake = data.earthquake;
        disableHardDrop = data.disableHardDrop;

        targetLines = data.targetLines;
        timeAttackLimit = data.timeAttackLimit;

        minDropDelay = data.minDropDelay;
        maxDropDelay = data.maxDropDelay;
        infectionInterval = data.infectionInterval;
        infectedTile = data.infectedTile;
        earthquakeInterval = data.earthquakeInterval;

        UIManager.Instance.UpdateStageIcon(data.stageIcon);

        ResetStage();
    }

    public void ResetStage()
    {
        clearedLines = 0;
        isStageCleared = false;
        infectionTimer = 0f;
        earthquakeTimer = 0f;

        stageTimer = isTimeAttack ? timeAttackLimit : 0f;
    }

    private void Update()
    {
        if (isStageCleared || GameManager.Instance.isGameOver) return;

        UpdateTimer();
        HandleVariableDropSpeed();
        HandleInfection();
        HandleEarthquake();
    }

    #region Timer & Line Clear Logic
    private void UpdateTimer()
    {
        if (isTimeAttack)
        {
            stageTimer -= Time.deltaTime;
            if (stageTimer <= 0f)
            {
                stageTimer = 0f;
                GameManager.Instance.GameOver();
            }
        }
        else
        {
            stageTimer += Time.deltaTime;
        }

        UIManager.Instance.UpdateTimerUI(stageTimer, isTimeAttack);
    }

    public void AddClearedLines(int count)
    {
        clearedLines += count;
        UIManager.Instance.UpdateLinesUI(clearedLines, targetLines);

        if (clearedLines >= targetLines)
        {
            StageClear();
        }
    }

    private void StageClear()
    {
        isStageCleared = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowStageClear();
    }
    #endregion

    #region Gimmick Logic
    private void HandleVariableDropSpeed()
    {
        if (!variableDropSpeed) return;

        Piece piece = GameManager.Instance.piece;
        if (piece == null) return;

        float t = Mathf.PingPong(Time.time * 0.5f, 1f);
        piece.stepDelay = Mathf.Lerp(minDropDelay, maxDropDelay, t);
    }

    private void HandleInfection()
    {
        if (!infectedBlock) return;

        infectionTimer += Time.deltaTime;
        if (infectionTimer >= infectionInterval)
        {
            infectionTimer = 0f;
            SpreadInfection();
        }
    }

    private void SpreadInfection()
    {
        Board board = GameManager.Instance.board;
        if (board == null) return;

        RectInt bounds = board.Bounds;
        occupiedPositionsBuffer.Clear();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (board.tilemap.HasTile(pos))
                {
                    occupiedPositionsBuffer.Add(pos);
                }
            }
        }

        if (occupiedPositionsBuffer.Count > 0)
        {
            Vector3Int targetPos = occupiedPositionsBuffer[Random.Range(0, occupiedPositionsBuffer.Count)];
            board.tilemap.SetTile(targetPos, infectedTile);
        }
    }

    private void HandleEarthquake()
    {
        if (!earthquake) return;

        earthquakeTimer += Time.deltaTime;
        if (earthquakeTimer >= earthquakeInterval)
        {
            earthquakeTimer = 0f;
            StartCoroutine(EarthquakeRoutine());
        }
    }

    private IEnumerator EarthquakeRoutine()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        if (mainCamera == null) mainCamera = Camera.main;

        while (elapsed < duration)
        {
            if (mainCamera != null)
            {
                Vector3 randomOffset = (Vector3)Random.insideUnitCircle * 0.2f;
                mainCamera.transform.position = originalCameraPos + randomOffset;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCameraPos;
        }

        ShiftBoardRows();
    }

    private void ShiftBoardRows()
    {
        Board board = GameManager.Instance.board;
        if (board == null) return;

        RectInt bounds = board.Bounds;
        int randomRow = Random.Range(bounds.yMin, bounds.yMin + 5);
        int shiftDir = Random.value > 0.5f ? 1 : -1;

        TileBase[] tempRow = new TileBase[bounds.width];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            tempRow[x - bounds.xMin] = board.tilemap.GetTile(new Vector3Int(x, randomRow, 0));
        }

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            int newX = x + shiftDir;
            if (newX >= bounds.xMin && newX < bounds.xMax)
            {
                board.tilemap.SetTile(new Vector3Int(newX, randomRow, 0), tempRow[x - bounds.xMin]);
            }
        }
    }
    #endregion
}