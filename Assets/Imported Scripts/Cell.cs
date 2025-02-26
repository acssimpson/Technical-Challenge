using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Lean.Pool;

public class Cell : MonoBehaviour
{
    public enum VisualState
    {
        None = 0,
        ValidMove = 1,
        ValidAttack = 2
    }
    [SerializeField] Color32[] VisualStateColors;
    public Cell[] Neighbors;
    public Vector2Int Coords {  get; private set; }
    public Piece CurrentPiece { get; private set; }
    [SerializeField] Sprite[] _BackgroundImages;
    [SerializeField] SpriteRenderer _Background;
    [SerializeField] SpriteRenderer _Highlight;

    public void Init(Vector2Int coord)
    {
        Coords = coord;
        SetPiece(null);
        SetVisualState(VisualState.None);
        _Background.sprite = _BackgroundImages[(Coords.x + Coords.y) % 2];
    }

    public void SetPiece(Piece piece)
    {
        if (piece != null)
        {
            piece.SetPosition(Coords);
            piece.transform.parent = transform;
            piece.transform.localPosition = Vector3.zero;
        }
        CurrentPiece = piece;

    }

    public void SetVisualState(VisualState state)
    {
        _Highlight.color = VisualStateColors[(int)state]; //Change the color of the overlay.
    }
}
