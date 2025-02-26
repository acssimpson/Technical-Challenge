using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Piece_Pawn : Piece
{
    public override ID PieceID => ID.Pawn;
    public override bool Continuous => false;
    protected override List<GridManager.Directions> baseMoveDirections => new List<GridManager.Directions>()
    {
        GridManager.Directions.U
    };
    protected override List<GridManager.Directions> baseAttackDirections => new List<GridManager.Directions>()
    {
        GridManager.Directions.UL,
        GridManager.Directions.UR
    };
}
