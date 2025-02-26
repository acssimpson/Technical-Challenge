using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Drawing;

public class GridManager : MonoBehaviour
{
    [SerializeField] public Vector2Int GridSize;
    private Vector2 _offset => new(GridSize.x/2f, GridSize.y/2f); 
    [SerializeField] float CellSize;
    [SerializeField] Transform _Anchor;

    [SerializeField] Cell prefab_Cell;

    public Dictionary<Vector2Int, Cell> CellDict = new();

    [SerializeField] Piece[] prefab_Pieces;

    public Dictionary<Piece, (List<Vector2Int> moveCells, List<Vector2Int> attackCells)> PieceList = new Dictionary<Piece, (List<Vector2Int>, List<Vector2Int>)>();


    /// <summary>
    /// Input Juggling
    /// </summary>

    public enum Directions
    {
        R = 0,
        DR = 1,
        D = 2,
        DL = 3,
        L = 4,
        UL = 5,
        U = 6,
        UR = 7
    }

    public static Vector2Int[] DirectionMap = new Vector2Int[8]
    {
        new(1,0),
        new(1,-1),
        new(0,-1),
        new(-1,-1),
        new(-1,0),
        new(-1,1),
        new(0,1),
        new(1,1)
    };

    // Start is called before the first frame update
    public void Init()
    {
        GenerateGrid();
        GeneratePieces();
    }

    // Update is called once per frame

    public void NewTurn()
    {
        foreach(var key in PieceList.Keys)
        {
            PieceList[key].moveCells.Clear();
            PieceList[key].attackCells.Clear();
        }
    }

    private void GenerateGrid()
    {
        //Just reuse the board on a reset
        if (CellDict.Count == 0)
        {
            for(int y = 0; y< GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    var cell = LeanPool.Spawn(prefab_Cell, _Anchor);
                    Vector2Int coord = new Vector2Int(x, y);
                    cell.transform.localPosition = (Vector2)coord *CellSize;

                    CellDict.Add(coord, cell);
                }
            }
        } 
        foreach(var kvp in CellDict)
        {
            kvp.Value.Init(kvp.Key, this);
        }
    }

    private void GeneratePieces()
    {
        //We don't know which ones have been used, so clear the board befor respawning all of them.

        if(PieceList.Count != 0)
        {
            foreach(var piece in PieceList)
            {
                LeanPool.Despawn(piece.Key);
            }
            PieceList.Clear();
        }


        for(int y = 0;y< GridSize.y; y++)
        {
            for(int x =0; x< GridSize.x; x++)
            {
                Piece piece = null;
                var coord = new Vector2Int(x, y);
                if (y == 0)
                {
                    piece = LeanPool.Spawn(prefab_Pieces[1], GetCell(coord).transform);
                    piece.Init(GameManager.ChessColor.White, coord);
                }    
                if (y == 1)
                {
                    piece = LeanPool.Spawn(prefab_Pieces[0], GetCell(coord).transform);
                    piece.Init(GameManager.ChessColor.White, coord);
                }                
                if (y == GridSize.y-2)
                {
                    piece = LeanPool.Spawn(prefab_Pieces[0], GetCell(coord).transform);
                    piece.Init(GameManager.ChessColor.Black, coord);
                }                
                if (y == GridSize.y-1)
                {
                    piece = LeanPool.Spawn(prefab_Pieces[1], GetCell(coord).transform);
                    piece.Init(GameManager.ChessColor.Black, coord);
                }
                if (piece != null)
                {
                    PieceList.Add(piece, (new List<Vector2Int>(), new List<Vector2Int>()));
                    GetCell(new Vector2Int(x, y)).SetPiece(piece);
                }
            }
        }    
    }
    
    public Cell GetCell(Vector2Int coord) => CellDict.TryGetValue(coord, out Cell cell) ? cell : null;

    public Cell GetCellFromScreenPoint(Vector3 point)
    {
        var delta = Camera.main.ScreenToWorldPoint(point) - _Anchor.position;
        var coord = delta.ToXY();
        coord /= CellSize;
        coord += new Vector2(.5f, .5f); //Offset to corner of square
        coord += DirectionMap[(int)Directions.UR]; //To prevent -.9 and 9 both flooring to zero
        return GetCell(new Vector2Int((int)coord.x, (int)coord.y) - DirectionMap[(int)Directions.UR]);
    }

    public void CalculatePieceMovement(Piece piece)
    {
        var validCells = GetValidDirectionalCells(piece);
        ShowPieceMovment(validCells);
    }

    public void ShowPieceMovment((List<Vector2Int> moveCells, List<Vector2Int> attackCells) validCells)
    {
        foreach (var moveCell in validCells.moveCells)
            GetCell(moveCell).SetVisualState(Cell.VisualState.ValidMove);

        Debug.Log($"attackcells {validCells.attackCells.Count}");
        foreach (var attackCell in validCells.attackCells)
            GetCell(attackCell).SetVisualState(Cell.VisualState.ValidAttack);

    }

    public void ClearCellVisuals()
    {
        foreach (var cell in CellDict.Values)
        {
            cell.SetVisualState(Cell.VisualState.None);
        }
    }

    public (List<Vector2Int> moveCells, List<Vector2Int> attackCells) GetValidDirectionalCells(Piece piece)
    {
        List<Vector2Int> moveCells = new();
        List<Vector2Int> attackCells = new();

        bool moveAttackOverlap = piece.ValidMove == piece.ValidAttack;
        foreach(var dir in piece.ValidMove)
        {
            Debug.Log($"Direction: {dir.ToString()}");
            //Flip the direction. This should works for all directional pieces.
            Vector2Int direction = DirectionMap[(int)dir] * (piece.PieceColor == GameManager.ChessColor.Black ? -1 : 1);
            bool valid = true;
            for(int i =1; valid; i++)
            {
                valid = piece.Continuous; //instantly back out if we are using a non-"directional" piece (aka a pawn)
                Vector2Int testPos = piece.Position + (i * direction);
                Cell cell = GetCell(testPos);
                if (cell != null)
                {
                    if (cell.CurrentPiece == null)
                    {
                        moveCells.Add(testPos);
                    }
                    else 
                    {
                        valid = false;
                        if (piece.ValidAttack.Contains(dir) && cell.CurrentPiece.PieceColor == piece.PieceColor.Opposite()) //This is the first step of making this dynamically evaluate attack/move sets, but isn't *actually* needed until we don't hardcode the pawn behavior below
                            attackCells.Add(testPos);
                    }
                }
                   
                else break;
            }
        }
        if (piece.PieceID == Piece.ID.Pawn) //This is simpler/cheaper than seeing if the attack & move set differ, but should be expanded if we add new movement/attack patterns
        {
            foreach (var dir in piece.ValidAttack)
            {
                Vector2Int testPos = piece.Position + DirectionMap[(int)dir] * (piece.PieceColor == GameManager.ChessColor.Black ? -1 : 1);
                Cell cell = GetCell(testPos);
                if(cell!=null && cell.CurrentPiece != null && cell.CurrentPiece.PieceColor == piece.PieceColor.Opposite())
                    attackCells.Add(testPos);
            }
        }
        PieceList[piece] = (moveCells, attackCells);
        return (moveCells, attackCells);

    }
}

