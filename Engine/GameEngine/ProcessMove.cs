using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        public void ProcessMove(Move move) => ProcessMove(move, CurrentState, simulation: false);

        /// <summary>
        /// Apply the consequences of a single move (captures, movement, tiles, piece hooks).
        /// NOTE: No end-of-turn and no turn-advance in here.
        /// </summary>
        private void ProcessMove(Move move, GameState currentState, bool simulation)
        {
            // Always work with the freshest snapshot as we commit events.
            currentState = CurrentState;

            var mover = currentState.Board.GetPieceAt(move.From);
            if (mover == null)
                return;

            // --- 1) Capture (emit PieceCaptured and target's OnCapture) ---
            var captured = currentState.Board.GetPieceAt(move.To);
            if (captured != null)
            {
                // Precompute the target’s OnCapture events from the current snapshot
                var onCapture = captured.OnCapture(currentState).ToList();

                var capEvent = new CandidateEvent(
                    GameEventType.PieceCaptured,
                    true,
                    new CapturePayload(captured)
                );

                var capRes = Commit(capEvent, simulation: simulation);
                if (capRes == null || capRes.Type == GameEventType.MoveCancelled)
                    return;

                // Apply each follow-up candidate from the captured piece
                foreach (var follow in onCapture)
                {
                    var extra = Commit(follow, simulation: simulation);
                    if (extra == null || extra.Type == GameEventType.MoveCancelled)
                        return;
                }

                currentState = CurrentState; // refresh snapshot
            }

            // --- 2) Apply the move itself ---
            var moveEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true,
                new MovePayload(mover, move.From, move.To)
            );

            var moveRes = Commit(moveEvent, simulation: simulation);
            if (moveRes == null || moveRes.Type == GameEventType.MoveCancelled)
                return;
            currentState = CurrentState;

            // After applying the MoveApplied event
            if (mover is Pawn pawn)
            {
                var promotionRank =
                    (pawn.Owner == PlayerColor.White) ? currentState.Board.Height - 1 : 0;

                if (move.To.Y == promotionRank)
                {
                    var queen = new Queen(pawn.Owner, move.To);

                    // Replace pawn with queen
                    var promoteEvent = new CandidateEvent(
                        GameEventType.PiecePromoted,
                        false,
                        new PiecePromotedPayload(pawn, queen, move.To)
                    );
                    Commit(promoteEvent);
                }
            }

            // --- 3) Piece-specific post-move effects ---
            foreach (var ev in mover.OnMove(move, currentState))
            {
                var extra = Commit(ev, simulation: simulation);
                if (extra == null || extra.Type == GameEventType.MoveCancelled)
                    return;
                currentState = CurrentState;
            }

            // --- 4) Tile entry effects (on the destination tile) ---
            var destTile = currentState.Board.GetTile(move.To);
            foreach (var ev in destTile.OnEnter(mover, move.To, currentState))
            {
                var extra = Commit(ev, simulation: simulation);
                if (extra == null || extra.Type == GameEventType.MoveCancelled)
                    return;
                currentState = CurrentState;
            }
        }
    }
}
