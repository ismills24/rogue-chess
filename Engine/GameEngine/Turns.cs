using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// Run one complete turn: TurnStart → Select/Process Move → TurnEnd → Advance Turn.
        /// </summary>
        public void RunTurn()
        {
            if (IsGameOver())
                return;

            // Turn-start effects for CURRENT player
            foreach (var candidateEvent in TickTurnStart(CurrentState))
                Commit(candidateEvent);

            // Get move from CURRENT player
            var controller =
                CurrentState.CurrentPlayer == PlayerColor.White
                    ? _whiteController
                    : _blackController;

            var move = controller.SelectMove(CurrentState);
            if (move == null)
                return; // no move chosen (e.g., human waiting)

            // Process the move (no advancing here)
            ProcessMove(move, CurrentState, simulation: false);

            // End-of-turn effects for the SAME player
            foreach (var ev in TickTurnEnd(CurrentState))
                Commit(ev);

            // Advance the turn
            var afterEnd = CurrentState;
            var nextPlayer =
                afterEnd.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;

            var advance = new CandidateEvent(
                GameEventType.TurnAdvanced,
                false,
                new TurnAdvancedPayload(nextPlayer, afterEnd.TurnNumber + 1)
            );
            Commit(advance);
        }

        /// <summary>
        /// Tick turn-start effects for all pieces and tiles of the current player.
        /// </summary>
        private IEnumerable<CandidateEvent> TickTurnStart(GameState state)
        {
            var currentPlayer = state.CurrentPlayer;

            // Tiles tick
            foreach (var piece in state.Board.GetAllPieces(currentPlayer))
            {
                var tile = state.Board.GetTile(piece.Position);
                foreach (var ev in tile.OnTurnStart(piece, piece.Position, state))
                    yield return ev;
            }

            // Pieces tick
            foreach (var piece in state.Board.GetAllPieces(currentPlayer))
            {
                foreach (var ev in piece.OnTurnStart(state))
                    yield return ev;
            }
        }

        /// <summary>
        /// Tick end-of-turn effects for all pieces of the current player.
        /// </summary>
        private IEnumerable<CandidateEvent> TickTurnEnd(GameState state)
        {
            foreach (var piece in state.Board.GetAllPieces(state.CurrentPlayer))
            {
                foreach (var ev in piece.OnTurnEnd(state))
                    yield return ev;
            }
        }
    }
}



