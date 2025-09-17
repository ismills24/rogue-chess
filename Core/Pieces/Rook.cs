using ChessRogue.Core;
using ChessRogue.Core.Rules;

public class Rook : IPiece
{
    public PlayerColor Owner { get; private set; }
    public Vector2Int Position { get; set; }
    public string Name => "Rook";

    public Rook(PlayerColor owner, Vector2Int startPos)
    {
        Owner = owner;
        Position = startPos;
    }

    public IEnumerable<Move> GetLegalMoves(GameState state)
    {
        return MovementRules.SlidingMoves(
            state,
            this,
            new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }
        );
    }

    public void OnMove(Move move, GameState state) { }

    public void OnCapture(GameState state) { }
}
