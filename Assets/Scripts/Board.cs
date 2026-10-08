using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    [Header("Game Over")]
    public Tile gameOverTile;

    [Header("Preview")]
    public Tilemap previewTilemap;
    public Vector3Int previewPosition = new Vector3Int(-15, 4, 0);
    public TetrominoData nextPieceData { get; private set; }

    [Header("Hold")]
    // 위치와 타일맵
    public Tilemap holdTilemap;
    public Vector3Int holdPosition = new Vector3Int(-15, -4, 0);

    // 상태
    public TetrominoData heldPieceData { get; private set; }
    public bool hasHeldPiece { get; private set; } = false;
    public bool canHold { get; private set; } = true;

    [Header("Score")]
    public int[] lineScores = { 0, 100, 300, 500, 800 }; // 1줄, 2줄, 3줄, 4줄
    public float tSpinValue;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SetNextPiece();

        SpawnPiece();
    }

    public void SpawnPiece()
    {
        canHold = true;

        // 미리 뽑아둔 nextPieceData로 현재 조각을 생성
        TetrominoData currentData = nextPieceData;
        activePiece.Initialize(this, spawnPosition, currentData);

        // 다음에 등장할 조각을 미리 뽑고 Preview 갱신
        SetNextPiece();

        if (IsValidPosition(activePiece, spawnPosition)) { Set(activePiece); }
        else
        {
            SetPartialGameOverPiece(activePiece, spawnPosition);
            GameOver();
        }
    }

    public void GameOver()
    {
        GameManager.Instance.GameOver();

        // Do anything else you want on game over here..
    }

    void SetPartialGameOverPiece(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (bounds.Contains((Vector2Int)tilePosition) && !tilemap.HasTile(tilePosition))
            {
                tilemap.SetTile(tilePosition, piece.data.tile);
            }
            else
            {
                TileBase tileToSet = (gameOverTile != null) ? gameOverTile : piece.data.tile;
                tilemap.SetTile(tilePosition, tileToSet);
            }
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool WillNextPieceOverlap()
    {
        for (int i = 0; i < nextPieceData.cells.Length; i++)
        {
            Vector3Int checkPos = (Vector3Int)nextPieceData.cells[i] + spawnPosition;

            // 다음 블록이 위치할 칸 중 하나라도 이미 타일이 차 있다면 true
            if (tilemap.HasTile(checkPos)) { return true; }
        }

        return false;
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines(bool isTSpin = false)
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }

        if (isTSpin) { GameManager.Instance.AddScore((int)(lineScores[linesCleared] * tSpinValue)); }
        else if (linesCleared > 0) { GameManager.Instance.AddScore(lineScores[linesCleared]); }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    #region Preview
    void SetNextPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        nextPieceData = tetrominoes[random];

        RenderPreview();
    }

    void RenderPreview()
    {
        if (previewTilemap == null) { return; }

        // 기존 미리 보기 타일을 지움.
        previewTilemap.ClearAllTiles();

        // 선택된 nextPieceData의 셀 정보로 타일 배치
        for (int i = 0; i < nextPieceData.cells.Length; i++)
        {
            Vector3Int tilePosition = (Vector3Int)nextPieceData.cells[i] + previewPosition;
            previewTilemap.SetTile(tilePosition, nextPieceData.tile);
        }
    }
    #endregion

    #region Hold
    public void HoldPiece()
    {
        if (!canHold) { return; }

        Clear(activePiece);

        if (!hasHeldPiece)
        {
            // 홀드 상자가 비어있으면 현재 조각 저장 후 새 조각 스폰
            heldPieceData = activePiece.data;
            hasHeldPiece = true;
            SpawnPiece();
        }
        else
        {
            // 이미 홀드된 조각이 있으면 현재 조각과 홀드 조각 교체 (Swap)
            TetrominoData temp = activePiece.data;
            activePiece.Initialize(this, spawnPosition, heldPieceData);
            heldPieceData = temp;

            if (!IsValidPosition(activePiece, spawnPosition)) { GameOver(); }
            else { Set(activePiece); }
        }

        canHold = false;

        RenderHold();
    }

    // Hold Tilemap에 홀드된 조각 그리기
    void RenderHold()
    {
        if (holdTilemap == null) { return; }

        holdTilemap.ClearAllTiles();

        for (int i = 0; i < heldPieceData.cells.Length; i++)
        {
            Vector3Int tilePosition = (Vector3Int)heldPieceData.cells[i] + holdPosition;
            holdTilemap.SetTile(tilePosition, heldPieceData.tile);
        }
    }
    #endregion

    #region T-Spin
    public bool CheckTSpin(Piece piece)
    {
        // 1. T 조각이 아니면 감지하지 않음
        if (piece.data.tetromino != Tetromino.T) { return false; }

        // 2. 마지막 동작이 회전이 아니었으면 감지하지 않음
        if (!piece.isLastMoveRotate) { return false; }

        // 3. T 조각 중심(0, 0) 기준 4개 대각선 모서리 위치 검사
        Vector3Int center = piece.position;
        Vector3Int[] corners = new Vector3Int[]
        {
        center + new Vector3Int(-1,  1, 0), // 대각선 좌상
        center + new Vector3Int( 1,  1, 0), // 대각선 우상
        center + new Vector3Int(-1, -1, 0), // 대각선 좌하
        center + new Vector3Int( 1, -1, 0)  // 대각선 우하
        };

        int occupiedCorners = 0;
        RectInt bounds = Bounds;

        foreach (Vector3Int corner in corners)
        {
            // 보드 경계 밖이거나 이미 타일이 차 있는 경우 충돌로 간주
            if (!bounds.Contains((Vector2Int)corner) || tilemap.HasTile(corner))
            {
                occupiedCorners++;
            }
        }

        // 4개 모서리 중 3개 이상 채워져 있다면 T-Spin 성공
        return occupiedCorners >= 3;
    }
    #endregion
}
