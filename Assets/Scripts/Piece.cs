using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{

    public enum ID
    {
        Pawn = 0,
        Queen = 1
    }

    protected abstract List<GridManager.Directions> baseMoveDirections { get;  }
    private List<Vector2Int> _moveOptions;
    public List<Vector2Int> MoveOptions
    {

        get
        {
            if (_moveOptions == null)
            {
                _moveOptions = new();
                foreach (var dir in baseMoveDirections)
                {
                    _moveOptions.Add(GridManager.DirectionMap[(int)dir] * (PieceColor == GameManager.ChessColor.Black ? -1 : 1));
                }
            }
            return _moveOptions;
        }
    }
    protected abstract List<GridManager.Directions> baseAttackDirections { get; }
    private List<Vector2Int> _attackOptions;
    public List<Vector2Int> AttackOptions
    {

        get
        {
            if (_attackOptions == null)
            {
                _attackOptions = new();
                foreach (var dir in baseAttackDirections)
                {
                    _attackOptions.Add(GridManager.DirectionMap[(int)dir] * (PieceColor == GameManager.ChessColor.Black ? -1 : 1));
                }
            }
            return _attackOptions;
        }
    }
    private List<Vector2Int> calculatedMoves = new();
    private List<Vector2Int> calculatedAttacks = new();
    public bool Clean {  get; private set; }
    public abstract bool Continuous { get; }


    protected GameManager.ChessColor _color;
    public GameManager.ChessColor PieceColor { get 
        {
            return _color;
        }
        set
        {
            _color = value;
            SpriteRenderer.color = _color == GameManager.ChessColor.White ? Color.white : Color.black;
        }
    }
    public abstract ID PieceID { get; }

    public Vector2Int Position { get; protected set; }

    [SerializeField] public SpriteRenderer SpriteRenderer;
    public void SetPosition(Vector2Int position) => Position = position;
    public void Init(GameManager.ChessColor color, Vector2Int position)
    {
        PieceColor = color;
        _moveOptions = null;
        SetPosition(position);
        ResetMovesets();
    }
    public void ResetMovesets()
    {
        calculatedMoves.Clear();
        calculatedAttacks.Clear();
        Clean = true;
    }
    public void SetMovesets(List<Vector2Int> calculatedMoves, List<Vector2Int> calculatedAttacks)
    {
        this.calculatedMoves = calculatedMoves;
        this.calculatedAttacks = calculatedAttacks;
        Clean = false;
    }
    public List<Vector2Int> GetMoves()
    {
        //If this was going to be more robust, this is where we catch an exception if clean==true
        return calculatedMoves;
    }
    public List<Vector2Int> GetAttacks()
    {
        //If this was going to be more robust, this is where we catch an exception if clean==true
        return calculatedAttacks;
    }
}
