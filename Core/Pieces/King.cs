using ChessRogue.Core;
using ChessRogue.Core.Rules;

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

    public IEnumerable<Move> GetLegalMoves(GameState state)
    {
        return MovementRules.AdjacentMoves(state, this, includeDiagonals: true);
    }

    public void OnMove(Move move, GameState state) { }

    public void OnCapture(GameState state) { }
}
