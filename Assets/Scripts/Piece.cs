using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    [Header("Settings")]
    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    [Header("Key Controls")]
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode right = KeyCode.RightArrow;
    public KeyCode softDrop = KeyCode.DownArrow;
    public KeyCode hardDrop = KeyCode.Space;
    public KeyCode cwRotate = KeyCode.X;
    public KeyCode acwRotate = KeyCode.Z;
    public KeyCode uTurnRotate = KeyCode.A;
    public KeyCode hold = KeyCode.C;

    [Header("Drop Score")]
    public int hardDropValue = 2;
    int dropDistance;

    public bool isLastMoveRotate { get; private set; }

    float stepTime;
    float moveTime;
    float lockTime;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;

        dropDistance = 0;
        isLastMoveRotate = false;
        rotationIndex = 0;

        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null || cells.Length != data.cells.Length)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        board.Clear(this);

        lockTime += Time.deltaTime;

        if (Input.GetKeyDown(hold))
        {
            board.HoldPiece();
            return;
        }

        if (Input.GetKeyDown(acwRotate)) Rotate(-1);
        else if (Input.GetKeyDown(cwRotate)) Rotate(1);
        else if (Input.GetKeyDown(uTurnRotate)) Rotate(2);

        if (Input.GetKeyDown(hardDrop))
        {
            if (StageManager.Instance == null || !StageManager.Instance.disableHardDrop)
            {
                HardDrop();
                return;
            }
        }

        if (Time.time > moveTime) HandleMoveInputs();
        if (Time.time > stepTime) Step();

        board.Set(this);
    }

    private void HandleMoveInputs()
    {
        if (Input.GetKey(softDrop))
        {
            if (Move(Vector2Int.down))
            {
                dropDistance++;
                stepTime = Time.time + stepDelay;
                GameManager.Instance.AddScore(1);
            }
        }

        if (Input.GetKey(left)) Move(Vector2Int.left);
        else if (Input.GetKey(right)) Move(Vector2Int.right);
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);

        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            dropDistance++;
        }
        GameManager.Instance.AddScore(dropDistance * hardDropValue);
        Lock();
    }

    private void Lock()
    {
        board.Set(this);
        bool isTSpin = board.CheckTSpin(this);
        board.ClearLines(isTSpin);
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position + (Vector3Int)translation;

        if (board.IsValidPosition(this, newPosition))
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f;
            isLastMoveRotate = false;
            return true;
        }

        return false;
    }

    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
        else
        {
            isLastMoveRotate = true;
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;
        int steps = Mathf.Abs(direction);
        int dir = direction > 0 ? 1 : -1;

        for (int step = 0; step < steps; step++)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 cell = cells[i];
                int x, y;

                switch (data.tetromino)
                {
                    case Tetromino.I:
                    case Tetromino.O:
                        cell.x -= 0.5f;
                        cell.y -= 0.5f;
                        x = Mathf.CeilToInt((cell.x * matrix[0] * dir) + (cell.y * matrix[1] * dir));
                        y = Mathf.CeilToInt((cell.x * matrix[2] * dir) + (cell.y * matrix[3] * dir));
                        break;

                    default:
                        x = Mathf.RoundToInt((cell.x * matrix[0] * dir) + (cell.y * matrix[1] * dir));
                        y = Mathf.RoundToInt((cell.x * matrix[2] * dir) + (cell.y * matrix[3] * dir));
                        break;
                }

                cells[i] = new Vector3Int(x, y, 0);
            }
        }
    }

    bool TestWallKicks(int rotIndex, int rotDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotIndex, rotDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if (Move(translation)) return true;
        }

        return false;
    }

    int GetWallKickIndex(int rotIndex, int rotDirection)
    {
        int wallKickIndex = rotIndex * 2;
        if (rotDirection < 0) wallKickIndex--;
        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    int Wrap(int input, int min, int max)
    {
        int range = max - min;
        return min + (((input - min) % range + range) % range);
    }
}