using ChessRogue.Core.Events;

namespace ChessRogue.Core.StatusEffects
{
    public interface IStatusEffect
    {
        string Name { get; }
        int Duration { get; }

        // Called once per turn, e.g. burning ticks down
        IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state);

        // Called if piece dies/captured (optional)
        IEnumerable<GameEvent> OnRemove(IPiece piece, GameState state);

        IStatusEffect Clone();
    }
}
