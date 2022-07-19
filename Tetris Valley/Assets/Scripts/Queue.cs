using UnityEngine;
using UnityEngine.Tilemaps;

public class Queue : MonoBehaviour
{
    public Board board;
    public Tile tile;
    public Piece nextPiece;
    public Vector3Int queuePosition;
    public Vector3Int[] cells { get; private set; }
    public Tilemap tilemap { get; private set; }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4]; //custom shape will change
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Set();
    }

    private void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.queuePosition;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.nextPiece.cells[i];
        }
    }

    private void Set()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.queuePosition;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }

}
