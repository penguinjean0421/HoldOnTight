// using System.Collections<Vector3Int>;
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
    public Tilemap holdTilemap;
    public Vector3Int holdPosition = new Vector3Int(-15, -4, 0);

    public TetrominoData heldPieceData { get; private set; }
    public bool hasHeldPiece { get; private set; }
    public bool canHold { get; private set; } = true;

    [Header("Score")]
    public int[] lineScores = { 0, 100, 300, 500, 800 };
    public float tSpinValue = 1.5f;

    // T-Spin 검사용 캐시 배열 (GC Alloc 방지)
    private static readonly Vector3Int[] TSpinCorners = new Vector3Int[]
    {
        new Vector3Int(-1,  1, 0),
        new Vector3Int( 1,  1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int( 1, -1, 0)
    };

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    public void ResetBoard()
    {
        tilemap.ClearAllTiles();
        if (previewTilemap != null) previewTilemap.ClearAllTiles();
        if (holdTilemap != null) holdTilemap.ClearAllTiles();

        hasHeldPiece = false;
        canHold = true;

        if (activePiece != null && activePiece.cells != null)
        {
            Clear(activePiece);
        }

        SetNextPiece();
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        canHold = true;

        TetrominoData currentData = nextPieceData;
        activePiece.Initialize(this, spawnPosition, currentData);

        SetNextPiece();

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameManager.Instance.GameOver();
        }
    }

    public void Set(Piece piece)
    {
        if (piece == null || piece.cells == null) return;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        if (piece == null || piece.cells == null) return;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition)) return false;
            if (tilemap.HasTile(tilePosition)) return false;
        }

        return true;
    }

    public void ClearLines(bool isTSpin = false)
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
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

        if (linesCleared > 0)
        {
            int baseScore = lineScores[Mathf.Min(linesCleared, lineScores.Length - 1)];
            int finalScore = isTSpin ? (int)(baseScore * tSpinValue) : baseScore;

            GameManager.Instance.AddScore(finalScore);
            StageManager.Instance.AddClearedLines(linesCleared);
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            if (!tilemap.HasTile(new Vector3Int(col, row, 0)))
            {
                return false;
            }
        }

        return true;
    }

    void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            tilemap.SetTile(new Vector3Int(col, row, 0), null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                TileBase above = tilemap.GetTile(new Vector3Int(col, row + 1, 0));
                tilemap.SetTile(new Vector3Int(col, row, 0), above);
            }
            row++;
        }
    }

    #region Preview
    void SetNextPiece()
    {
        int random;
        bool sealI = StageManager.Instance != null && StageManager.Instance.sealIBlock;

        do
        {
            random = Random.Range(0, tetrominoes.Length);
        }
        while (sealI && tetrominoes[random].tetromino == Tetromino.I);

        nextPieceData = tetrominoes[random];
        RenderPreview();
    }

    void RenderPreview()
    {
        if (previewTilemap == null) return;

        previewTilemap.ClearAllTiles();

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
        if (!canHold) return;

        Clear(activePiece);

        if (!hasHeldPiece)
        {
            heldPieceData = activePiece.data;
            hasHeldPiece = true;
            SpawnPiece();
        }
        else
        {
            TetrominoData temp = activePiece.data;
            activePiece.Initialize(this, spawnPosition, heldPieceData);
            heldPieceData = temp;

            if (!IsValidPosition(activePiece, spawnPosition))
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Set(activePiece);
            }
        }

        canHold = false;
        RenderHold();
    }

    void RenderHold()
    {
        if (holdTilemap == null) return;

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
        if (piece.data.tetromino != Tetromino.T || !piece.isLastMoveRotate) return false;

        Vector3Int center = piece.position;
        int occupiedCorners = 0;
        RectInt bounds = Bounds;

        for (int i = 0; i < TSpinCorners.Length; i++)
        {
            Vector3Int corner = center + TSpinCorners[i];
            if (!bounds.Contains((Vector2Int)corner) || tilemap.HasTile(corner))
            {
                occupiedCorners++;
            }
        }

        return occupiedCorners >= 3;
    }
    #endregion
}