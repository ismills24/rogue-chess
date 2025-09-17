using ChessRogue.Core;
using ChessRogue.Core.Pieces;

public class Bishop : IPiece
{
    public PlayerColor Owner { get; private set; }
    public Vector2Int Position { get; set; }
    public string Name => "Bishop";

    public Bishop(PlayerColor owner, Vector2Int startPos)
    {
        Owner = owner;
        Position = startPos;
    }

    public IEnumerable<Move> GetPseudoLegalMoves(GameState state)
    {
        return Movement.SlidingMoves(
            state,
            this,
            new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1),
            }
        );
    }

    public void OnMove(Move move, GameState state) { }

    public void OnCapture(GameState state) { }

    public IPiece Clone()
    {
        return new Bishop(Owner, Position);
    }
}
