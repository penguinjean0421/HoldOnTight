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

    [Header("Preview")]
    public Tilemap previewTilemap;
    public Vector3Int previewPosition = new Vector3Int(-15, 4, 0);
    public TetrominoData nextPieceData { get; private set; }

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
        // 미리 뽑아둔 nextPieceData로 현재 조각을 생성
        TetrominoData currentData = nextPieceData;
        activePiece.Initialize(this, spawnPosition, currentData);

        // 다음에 등장할 조각을 미리 뽑고 Preview 갱신
        SetNextPiece();

        if (IsValidPosition(activePiece, spawnPosition)) { Set(activePiece); }
        else { GameOver(); }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();

        if (previewTilemap != null) { previewTilemap.ClearAllTiles(); }

        // Do anything else you want on game over here..
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

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
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

}
