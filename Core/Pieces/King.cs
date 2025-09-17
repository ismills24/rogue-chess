using ChessRogue.Core;
using ChessRogue.Core.Pieces;

public class King : IPiece
{
    public PlayerColor Owner { get; private set; }
    public Vector2Int Position { get; set; }
    public string Name => "King";

    public King(PlayerColor owner, Vector2Int startPos)
    {
        Owner = owner;
        Position = startPos;
    }

    public IEnumerable<Move> GetPseudoLegalMoves(GameState state)
    {
        return Movement.AdjacentMoves(state, this, includeDiagonals: true);
    }

    public void OnMove(Move move, GameState state) { }

    public void OnCapture(GameState state) { }

    public IPiece Clone()
    {
        return new King(Owner, Position);
    }
}
