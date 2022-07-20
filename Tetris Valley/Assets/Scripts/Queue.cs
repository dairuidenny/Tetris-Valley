using UnityEngine;
using UnityEngine.Tilemaps;

public class Queue : MonoBehaviour
{
    public Board board;
    public Tile tile;
    public Piece nextPiece;
    public Vector3Int queuePosition;
    public Vector3Int holdPosition;
    public Tilemap tilemap { get; private set; }
    public TetrominoData queueData { get; private set; }
    public TetrominoData holdData { get; private set; }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
    }

    private void LateUpdate()
    {
        ClearQueue();
        FindNext();
        SetQueue();

        ClearHold();
        UpdateHold();
        SetHold();
    }

    private void ClearQueue()
    {
        if (queueData.cells != null)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3Int tilePosition = (Vector3Int)queueData.cells[i] + this.queuePosition;
                this.tilemap.SetTile(tilePosition, null);
            }
        }

    }

    private void FindNext()
    {
        int index = nextPiece.Wrap(board.queueIndex + 1, 0, 7);
        queueData = board.tetrominoes[board.queue[index]];
    }

    private void SetQueue()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3Int tilePosition = (Vector3Int)queueData.cells[i] + this.queuePosition;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }

    private void ClearHold()
    {
        if (holdData.cells != null)
        {
            for (int i = -2; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Vector3Int tilePosition = new Vector3Int(i, j, 0) + this.holdPosition;
                    this.tilemap.SetTile(tilePosition, null);
                }
            }
        }
    }

    private void UpdateHold()
    {
        if (board.hold.Count > 0)
        {
            holdData = board.tetrominoes[board.hold[0]];
        }
    }

    private void SetHold()
    {
        if (board.hold.Count > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3Int tilePosition = (Vector3Int)holdData.cells[i] + this.holdPosition;

                if (board.hold[0] == 0) //Move I 1 tile left
                {
                    tilePosition += Vector3Int.left;
                }

                this.tilemap.SetTile(tilePosition, this.tile);
            }
        }

    }
}
