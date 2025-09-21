using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.StatusEffects
{
    /// <summary>
    /// Contract for all status effects that can be applied to pieces.
    /// </summary>
    public interface IStatusEffect
    {
        string Name { get; }

        /// <summary>
        /// Called at the start of the owning piece's turn.
        /// Should emit any CandidateEvents (ticks, damage, destruction, etc.).
        /// </summary>
        IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, GameState state);

        IEnumerable<CandidateEvent> OnTurnEnd(IPiece piece, GameState state);

        /// <summary>
        /// How much this status modifies the piece’s value (positive or negative).
        /// Default is 0.
        /// </summary>
        int ValueModifier();

        /// <summary>
        /// Deep clone for safe use in GameState cloning.
        /// </summary>
        IStatusEffect Clone();
    }
}



