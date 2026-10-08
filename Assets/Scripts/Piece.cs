using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    [Header("Play")]
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode right = KeyCode.RightArrow;
    public KeyCode softDrop = KeyCode.DownArrow;
    public KeyCode hardDrop = KeyCode.Space;
    public KeyCode cwRotate = KeyCode.X;
    public KeyCode acwRotate = KeyCode.Z;
    public KeyCode uTurnRotate = KeyCode.A;
    public KeyCode hold = KeyCode.C;

    [Header("Drop Score")]
    public int hardDropValue;
    int dropDistance;

    [Header("T Spin")]
    public bool isLastMoveRotate { get; private set; }

    private float stepTime;
    private float moveTime;
    private float lockTime;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        dropDistance = 0;
        isLastMoveRotate = false;

        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null) { cells = new Vector3Int[data.cells.Length]; }

        for (int i = 0; i < cells.Length; i++) { cells[i] = (Vector3Int)data.cells[i]; }
    }

    private void Update()
    {
        board.Clear(this);

        // We use a timer to allow the player to make adjustments to the piece
        // before it locks in place
        lockTime += Time.deltaTime;

        if (Input.GetKeyDown(hold)) { board.HoldPiece(); }

        // Handle rotation
        if (Input.GetKeyDown(acwRotate)) { Rotate(-1); }
        else if (Input.GetKeyDown(cwRotate)) { Rotate(1); }
        else if (Input.GetKeyDown(uTurnRotate)) { Rotate(2); }

        // Handle hard drop
        if (Input.GetKeyDown(hardDrop)) { HardDrop(); }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime) { HandleMoveInputs(); }

        // Advance the piece to the next row every x seconds
        if (Time.time > stepTime) { Step(); }

        board.Set(this);
    }

    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(softDrop))
        {
            if (Move(Vector2Int.down))
            {
                dropDistance++;

                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;

                GameManager.Instance.AddScore(1);
            }
        }



        // Left/right movement
        if (Input.GetKey(left)) { Move(Vector2Int.left); }
        else if (Input.GetKey(right)) { Move(Vector2Int.right); }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay) { Lock(); }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down)) { dropDistance++; }
        GameManager.Instance.AddScore(dropDistance * hardDropValue);

        Lock();
    }

    private void Lock()
    {
        board.Set(this);

        bool isTSpin = board.CheckTSpin(this);
        board.ClearLines(isTSpin);

        if (board.WillNextPieceOverlap()) { board.GameOver(); return; }
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset

            isLastMoveRotate = false;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
        else { isLastMoveRotate = true; }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        int steps = Mathf.Abs(direction);
        int dir = direction > 0 ? 1 : -1;

        // Rotate all of the cells using the rotation matrix
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

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

}
