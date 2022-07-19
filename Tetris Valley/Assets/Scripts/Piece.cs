using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;
    public float repeatDelay = 0.2f;

    private float stepTime;
    private float lockTime;
    private float repeatTime;
    private int repeatDir;

    private int moveCount;
    private int moveCountMax;
    private bool dpadUp;
    private bool dpadDown;
    private bool dpadLeft;
    private bool dpadRight;
    private bool canHold;


    public void Initialized(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.repeatTime = 0f;
        this.repeatDir = 0;
        this.moveCount = 0;
        this.moveCountMax = 15;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for(int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        this.board.Clear(this);
        this.lockTime += Time.deltaTime;

        //Move
        if (Input.GetButtonDown("Left") | (this.dpadLeft & Input.GetAxisRaw("DpadX") < 0f))
        {
            Move(Vector2Int.left);
            repeatTime = 0;
            repeatDir = -1;
            dpadLeft = false;
        }
        if (Input.GetButtonDown("Right") | (this.dpadRight & Input.GetAxisRaw("DpadX") > 0f))
        {
            Move(Vector2Int.right);
            repeatTime = 0;
            repeatDir = 1;
            dpadRight = false;
        }
        if (Input.GetAxisRaw("DpadX") == 0f)
        {
            this.dpadLeft = true;
            this.dpadRight = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) | Input.GetKeyUp(KeyCode.RightArrow))
        {
            repeatTime = 0;
            repeatDir = 0;
        }
        if (Input.GetKey(KeyCode.LeftArrow) | Input.GetAxisRaw("DpadX") < 0f)
        {
            repeatTime += Time.deltaTime;
            if (repeatTime >= repeatDelay & repeatDir != 1)
            {
                Move(Vector2Int.left);
            }
        }
        if (Input.GetKey(KeyCode.RightArrow) | Input.GetAxisRaw("DpadX") > 0f)
        {
            repeatTime += Time.deltaTime;
            if (repeatTime >= repeatDelay & repeatDir != -1)
            {
                Move(Vector2Int.right);
            }
        }

        //Rotate
        if (Input.GetButtonDown("RotateL"))
        {
            Rotate(-1);
        }
        if (Input.GetButtonDown("RotateR"))
        {
            Rotate(1);
        }

        //Soft drop
        if (Input.GetButton("SoftDrop") | Input.GetAxisRaw("DpadY") < -0f)
        {
            Move(Vector2Int.down);
        }

        //Hard drop
        if (Input.GetButtonDown("HardDrop") | (this.dpadUp & Input.GetAxisRaw("DpadY") > 0f))
        {
            Harddrop();
            this.dpadUp = false;
        }
        if (Input.GetAxisRaw("DpadY") == 0f)
        {
            this.dpadUp = true;
        }

        //Step
        if (Time.time >= this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;
        Move(Vector2Int.down);

        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private void Harddrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }
        Lock();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.isValidPosition(this, newPosition);

        if (valid)
        {
            if (translation.y >= 0 & IsTouchDown(this)) //touching surface
            {
                this.moveCount++;
            }
            else
            {
                this.moveCount = 0;
            }
            if (this.moveCount <= this.moveCountMax)
            {
                this.position = newPosition;
                this.lockTime = 0f;
            }
            else
            {
                valid = false;
            }
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex,direction))
        {
            this.rotationIndex = originRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < data.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];
            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool IsTouchDown(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int positionDown = piece.cells[i] + piece.position + Vector3Int.down;

            if (this.board.tilemap.HasTile(positionDown) | positionDown.y <= -this.board.BoardSize.y / 2)
            {
                return true;
            }
        }
        return false;
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationDirection * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    public int Wrap(int input, int min, int max)
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
