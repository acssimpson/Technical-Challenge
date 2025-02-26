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

    public abstract List<GridManager.Directions> ValidMove { get; }
    public abstract bool Continuous { get; }
    public abstract List<GridManager.Directions> ValidAttack { get; }


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
        SetPosition(position);
    }
}
