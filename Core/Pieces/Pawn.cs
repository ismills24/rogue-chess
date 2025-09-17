using ChessRogue.Core;
using ChessRogue.Core.Rules;

public class Pawn : IPiece
{
    public PlayerColor Owner { get; private set; }
    public Vector2Int Position { get; set; }
    public string Name => "Pawn";

    public Pawn(PlayerColor owner, Vector2Int startPos)
    {
        Owner = owner;
        Position = startPos;
    }

    public IEnumerable<Move> GetPseudoLegalMoves(GameState state)
    {
        int direction = (Owner == PlayerColor.White) ? 1 : -1;
        var moves = new List<Move>();

        // Standard forward moves
        moves.AddRange(MovementRules.ForwardMoves(state, this, 1, direction));

        // Double step from starting rank
        if (
            (Owner == PlayerColor.White && Position.y == 1)
            || (Owner == PlayerColor.Black && Position.y == 6)
        )
        {
            moves.AddRange(MovementRules.ForwardMoves(state, this, 2, direction));
        }

        // Captures
        moves.AddRange(MovementRules.DiagonalCaptures(state, this, direction));

        // En Passant
        moves.AddRange(MovementRules.EnPassantCaptures(state, this, direction));

        return moves;
    }

    public void OnMove(Move move, GameState state)
    {
        // Promotion
        if (
            (Owner == PlayerColor.White && move.To.y == 7)
            || (Owner == PlayerColor.Black && move.To.y == 0)
        )
        {
            var promoted = new Queen(Owner, move.To);
            state.Board.PlacePiece(promoted, move.To);
        }
    }

    public void OnCapture(GameState state) { }

    public IPiece Clone()
    {
        return new Pawn(Owner, Position);
    }
}
