using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Piece_Pawn : Piece
{
    public override ID PieceID => ID.Pawn;
    public override bool Continuous => false;
    public override List<GridManager.Directions> ValidMove => new List<GridManager.Directions>()
    {
        GridManager.Directions.U
    };
    public override List<GridManager.Directions> ValidAttack => new List<GridManager.Directions>()
    {
        GridManager.Directions.UL,
        GridManager.Directions.UR
    };
}
