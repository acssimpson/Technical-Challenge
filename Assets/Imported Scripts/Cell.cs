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
    public VisualState CurrentVisualState {  get; private set; }
    public Cell[] Neighbors;
    public Vector2Int Coords {  get; private set; }
    public Piece CurrentPiece { get; private set; }
    private GridManager _manager;
    [SerializeField] Sprite[] _BackgroundImages;
    [SerializeField] SpriteRenderer _Background;
    [SerializeField] SpriteRenderer _Highlight;
    public void Init(Vector2Int coord, GridManager manager)
    {
        _manager = manager;
        Coords = coord;
        SetPiece(null);
        SetVisualState(VisualState.None);
        _Background.sprite = _BackgroundImages[(Coords.x + Coords.y) % 2];
    }

    public void SetPiece(Piece piece)
    {
        //Handle state juggling here?
        if (piece != null)
        {
            if (CurrentPiece != null) LeanPool.Despawn(CurrentPiece); //Only occurs when taking a piece.    
            piece.SetPosition(Coords);
            piece.transform.parent = transform;
            piece.transform.localPosition = Vector3.zero;
        }
        CurrentPiece = piece;
    }

    public void SetVisualState(VisualState state)
    {
        CurrentVisualState = state;
        _Highlight.color = VisualStateColors[(int)CurrentVisualState];
        //Change the color of the overlay.
    }
    public void GatherNeighbors() 
    {
        int debugN = 0;
        for(int i = 0; i < Neighbors.Length; i++)
        {
            var n =_manager.GetCell(Coords+GridManager.DirectionMap[i]);
            Neighbors[i] = n;
            if (n != null) debugN++;
        }
    }
}
