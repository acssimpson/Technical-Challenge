using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Drawing;
using Unity.Burst.Intrinsics;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] public Vector2Int GridSize;
    [SerializeField] float CellSize;
    [SerializeField] Transform _Anchor;
    [SerializeField] Cell prefab_Cell;
    [SerializeField] Piece[] prefab_Pieces;


    public Dictionary<Vector2Int, Cell> CellDict = new();
    public List<Piece> PieceList = new();

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
        foreach (var piece in PieceList)
            piece.ResetMovesets();
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
            kvp.Value.Init(kvp.Key);
        }
    }

    private void GeneratePieces()
    {
        //We don't know which ones have been used, so clear the board befor respawning all of them.
        for (int i = PieceList.Count - 1; i >= 0; i--)
        {
            DespawnPiece(PieceList[i]); //Removed entries from piecelist, so we use a reverse for
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
                    PieceList.Add(piece);
                    GetCell(coord).SetPiece(piece);
                }
            }
        }    
    }

    public void DespawnPiece(Piece piece)
    {
        if (piece == null) return; //throw a theoretical exception here
        GetCell(piece.Position).SetPiece(null);
        if(PieceList.Contains(piece))
            PieceList.Remove(piece);

        LeanPool.Despawn(piece);
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
        GetValidDirectionalCells(piece);
        ShowPieceMovment(piece);
    }

    public void ShowPieceMovment(Piece piece)
    {
        foreach (var moveCell in piece.GetMoves())
            GetCell(moveCell).SetVisualState(Cell.VisualState.ValidMove);

        foreach (var attackCell in piece.GetAttacks())
            GetCell(attackCell).SetVisualState(Cell.VisualState.ValidAttack);
    }

    public void ClearCellVisuals()
    {
        foreach (var cell in CellDict.Values)
            cell.SetVisualState(Cell.VisualState.None);
    }

    public void GetValidDirectionalCells(Piece piece)
    {
        //if (!piece.Clean)
        //    return;

        //Note that the break delegates always fail if the piece is marked continuous, forcing a search range of 1.
        validCell moveCellEval = (cell) => (cell.CurrentPiece == null); //Include empty cells
        shouldBreak moveBreakEval = (cell) => (!piece.Continuous || !moveCellEval(cell)); //break as soon as we fail
        List<Vector2Int> moveCells = EvaluateDirectionSet(piece.Position, piece.MoveOptions, moveCellEval, moveBreakEval);

        validCell attackCellEval = (cell) => (cell.CurrentPiece != null && cell.CurrentPiece.PieceColor == piece.PieceColor.Opposite()); //include non-empty cells with enemies
        shouldBreak attackBreakEval = (cell) => (!piece.Continuous || cell.CurrentPiece!=null); //break after finding any non-empty cell
        List<Vector2Int> attackCells = EvaluateDirectionSet(piece.Position, piece.AttackOptions, attackCellEval, attackBreakEval);

        piece.SetMovesets(moveCells, attackCells);
    }

    public delegate bool validCell(Cell cell);
    public delegate bool shouldBreak(Cell cell);

    /// <summary>
    /// This is a genericized method for evaluation a direction set and applying the provided evaluation delegate, and then continuing or stopping based on the break delegate.
    /// </summary>
    /// <param name="evaluateCell">Takes the cell, evaluates if it should be added to the result.</param>
    /// <param name="shouldBreak">Takes a cell, evaluates if we should stop searching in the current direction</param>
    /// <returns></returns>
    private List<Vector2Int> EvaluateDirectionSet(Vector2Int origin, List<Vector2Int> optionSet, validCell evaluateCell, shouldBreak shouldBreak)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (var dir in optionSet)
        {
            for (int i = 1; true; i++)
            {
                Vector2Int testPos = origin + (i * dir);
                Cell cell = GetCell(testPos);
                if (cell != null)
                {
                    if (evaluateCell(cell)) result.Add(testPos);
                    if (shouldBreak(cell)) break;
                }
                else break;
            }
        }
        return result;
    }
}

