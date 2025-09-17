using ChessRogue.Core;
using ChessRogue.Core.Rules;

public class Knight : IPiece
{
    public PlayerColor Owner { get; private set; }
    public Vector2Int Position { get; set; }
    public string Name => "Knight";

    public Knight(PlayerColor owner, Vector2Int startPos)
    {
        Owner = owner;
        Position = startPos;
    }

    public IEnumerable<Move> GetPseudoLegalMoves(GameState state)
    {
        var offsets = new[]
        {
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-2, -1),
            new Vector2Int(1, 2),
            new Vector2Int(1, -2),
            new Vector2Int(-1, 2),
            new Vector2Int(-1, -2),
        };
        return MovementRules.JumpMoves(state, this, offsets);
    }

    public void OnMove(Move move, GameState state) { }

    public void OnCapture(GameState state) { }

    public IPiece Clone()
    {
        return new Knight(Owner, Position);
    }
}
