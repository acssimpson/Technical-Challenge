using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece_Queen : Piece
{
    public override ID PieceID => ID.Queen;
    public override bool Continuous => true;
    public override List<GridManager.Directions> ValidMove => new List<GridManager.Directions>()
    {
        GridManager.Directions.R,
        GridManager.Directions.DR,
        GridManager.Directions.D,
        GridManager.Directions.DL,
        GridManager.Directions.L,
        GridManager.Directions.UL,
        GridManager.Directions.U,
        GridManager.Directions.UR
    };
    public override List<GridManager.Directions> ValidAttack => ValidMove;

}
