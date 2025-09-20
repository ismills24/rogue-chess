using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Interfaces
{
    /// <summary>
    /// A tile is a single square on the board.
    /// It can react to pieces entering, starting a turn, or leaving.
    /// </summary>
    public interface ITile
    {
        Vector2Int Position { get; }

        /// <summary>
        /// Called when a piece enters this tile.
        /// Returns candidate game events (not yet confirmed).
        /// </summary>
        IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state);

        /// <summary>
        /// Called when a piece starts its turn while standing on this tile.
        /// </summary>
        IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state);

        /// <summary>
        /// Deep clone the tile (used for GameState snapshots).
        /// </summary>
        ITile Clone();
    }
}
