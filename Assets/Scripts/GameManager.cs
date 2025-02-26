using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum ChessColor
    {
        White = 0,
        Black = 1
    }

    [SerializeField] GridManager _GridManager;
    [SerializeField] Image mouseFollow;
    [SerializeField] TextMeshProUGUI TurnText;
    [SerializeField] TextMeshProUGUI VictoryText;
    [SerializeField] GameObject VictoryBanner;

    Piece currentDragPiece;
    public ChessColor ActivePlayer = ChessColor.White;
    bool won = false;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public void Init() //separated so we can attach a button to it.
    {
        won = false;
        SetPlayerTurn(ChessColor.White);
        _GridManager.Init();
        VictoryBanner.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (won)
            return;

        if (currentDragPiece != null) mouseFollow.rectTransform.localPosition = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            var piece = _GridManager.GetCellFromScreenPoint(Input.mousePosition)?.CurrentPiece;
            if (piece != null && piece.PieceColor == ActivePlayer)
            {
                _GridManager.CalculatePieceMovement(piece);

                currentDragPiece = piece;
                mouseFollow.gameObject.SetActive(true);
                mouseFollow.sprite = currentDragPiece.SpriteRenderer.sprite;
                mouseFollow.color = currentDragPiece.SpriteRenderer.color;
                Debug.Log($"Start drag {currentDragPiece.PieceID.ToString()}");
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentDragPiece != null)
            {
                var dragoverCell = _GridManager.GetCellFromScreenPoint(Input.mousePosition);
                if (dragoverCell != null)
                {
                    if (_GridManager.PieceList[currentDragPiece].moveCells.Contains(dragoverCell.Coords) || _GridManager.PieceList[currentDragPiece].attackCells.Contains(dragoverCell.Coords))
                    {
                        _GridManager.GetCell(currentDragPiece.Position).SetPiece(null);
                        dragoverCell.SetPiece(currentDragPiece);
                        ProgressTurn();
                    }
                }
                currentDragPiece = null;
                mouseFollow.gameObject.SetActive(false);
                _GridManager.ClearCellVisuals();
            //Kill mousefollow regardless.
            }
        }
    }

    private void SetPlayerTurn(ChessColor color)
    {
        ActivePlayer = color;
        TurnText.text = $"{ActivePlayer.ToString()}'S TURN";
    }
    public void ProgressTurn()
    {
        var max = (int)ChessColor.Black * (_GridManager.GridSize.y - 1);
        SetPlayerTurn(ActivePlayer.Opposite());
        foreach(var piece in _GridManager.PieceList.Keys)
        {
            if(piece.Position.y == ((int)piece.PieceColor.Opposite()) * (_GridManager.GridSize.y-1))
            {

                VictoryText.text = $"VICTORY FOR {piece.PieceColor}";
                VictoryBanner.SetActive(true);
                won = true;
                break;
            }
        }
        //Change Player
        //Reset move calculations

    }
}

public static class Helpers
{
    public static Vector2 ToXY(this Vector3 v) => new Vector2(v.x, v.y);
    public static GameManager.ChessColor Opposite(this GameManager.ChessColor c) => (c == GameManager.ChessColor.White) ? GameManager.ChessColor.Black : GameManager.ChessColor.White;
}
