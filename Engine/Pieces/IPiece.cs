using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Interfaces
{
    /// <summary>
    /// Base contract for all pieces (pawns, rooks, custom roguelike units, etc).
    /// Pieces can move, be cloned, and may expose abilities via decorators.
    /// </summary>
    public interface IPiece
    {
        string Name { get; }
        PlayerColor Owner { get; }
        Vector2Int Position { get; set; }
        int MovesMade { get; set; }
        int CapturesMade { get; set; }

        /// <summary>
        /// Return pseudo-legal moves from this piece in the given state.
        /// Note: legality is determined by the Ruleset.
        /// </summary>
        IEnumerable<Move> GetPseudoLegalMoves(GameState state);

        /// <summary>
        /// Called when this piece moves. Returns candidate events.
        /// </summary>
        IEnumerable<CandidateEvent> OnMove(Move move, GameState state);

        /// <summary>
        /// Called when this piece is captured. Returns candidate events.
        /// </summary>
        IEnumerable<CandidateEvent> OnCapture(GameState state);

        /// <summary>
        /// Get the base value of this piece for evaluation.
        /// </summary>
        int GetValue();

        IEnumerable<CandidateEvent> OnTurnStart(GameState state);
        IEnumerable<CandidateEvent> OnTurnEnd(GameState state);

        /// <summary>
        /// Deep clone the piece (used when cloning GameStates).
        /// </summary>
        IPiece Clone();
    }
}

