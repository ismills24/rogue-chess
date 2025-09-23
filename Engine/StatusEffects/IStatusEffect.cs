using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.StatusEffects
{
    /// <summary>
    /// Contract for all status effects that can be applied to pieces.
    /// Now emits canonical GameEvents instead of CandidateEvents.
    /// </summary>
    public interface IStatusEffect
    {
        string Name { get; }

        /// <summary>
        /// Called at the start of the owning piece's turn.
        /// Return one or more GameEvents (StatusAppliedEvent, DestroyEvent, etc).
        /// </summary>
        IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state);

        /// <summary>
        /// Called at the end of the owning piece's turn.
        /// </summary>
        IEnumerable<GameEvent> OnTurnEnd(IPiece piece, GameState state);

        int ValueModifier();

        IStatusEffect Clone();
    }
}
