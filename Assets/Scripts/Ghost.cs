using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board mainBoard;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        if (trackingPiece == null || mainBoard == null) return;

        Clear();
        Copy();
        CalculateDropPosition();
        Set();
    }

    private void Clear()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            tilemap.SetTile(cells[i] + position, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = trackingPiece.cells[i];
        }
    }

    // 메인 보드의 타일 상태를 훼손하지 않고 낙하 지점 계산
    private void CalculateDropPosition()
    {
        Vector3Int currentPos = trackingPiece.position;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        position = currentPos;

        for (int row = currentPos.y; row >= bottom; row--)
        {
            Vector3Int testPos = new Vector3Int(currentPos.x, row, 0);

            if (IsValidGhostPosition(testPos))
            {
                position = testPos;
            }
            else
            {
                break;
            }
        }
    }

    private bool IsValidGhostPosition(Vector3Int testPosition)
    {
        RectInt bounds = mainBoard.Bounds;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + testPosition;

            if (!bounds.Contains((Vector2Int)tilePosition)) return false;

            // 현재 추적 중인 피스 자체 위치가 아닐 때만 타일 검사
            if (mainBoard.tilemap.HasTile(tilePosition))
            {
                if (!IsCellOfTrackingPiece(tilePosition))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsCellOfTrackingPiece(Vector3Int tilePos)
    {
        for (int i = 0; i < trackingPiece.cells.Length; i++)
        {
            if (trackingPiece.cells[i] + trackingPiece.position == tilePos)
            {
                return true;
            }
        }
        return false;
    }

    private void Set()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            tilemap.SetTile(cells[i] + position, tile);
        }
    }
}