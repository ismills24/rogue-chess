using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// Commit a candidate event through hooks into one or more canonical events.
        /// </summary>
        public GameEvent? Commit(
            CandidateEvent candidate,
            Guid? parentEventId = null,
            bool simulation = false
        )
        {
            var currentState = CurrentState;
            var pending = new List<CandidateEvent> { candidate };

            // Run hooks; each can cancel or replace
            foreach (var hook in HookCollector.CollectHooks(currentState))
            {
                var nextPending = new List<CandidateEvent>();

                foreach (var cand in pending)
                {
                    var result = hook.BeforeEvent(cand, currentState);

                    if (result == null)
                    {
                        // Cancel this candidate entirely
                        continue;
                    }

                    var list = result.ToList();
                    if (list.Count == 0)
                    {
                        // Keep original
                        nextPending.Add(cand);
                    }
                    else
                    {
                        // Replace with hook outputs
                        nextPending.AddRange(list);
                    }
                }

                pending = nextPending;
                if (pending.Count == 0)
                    return null; // everything cancelled
            }

            GameEvent? last = null;

            foreach (var cand in pending)
            {
                var canonical = new GameEvent(
                    Guid.NewGuid(),
                    cand.Type,
                    cand.IsPlayerAction,
                    parentEventId,
                    cand.Payload
                );

                var newState = ApplyEventToState(canonical, currentState);

                // manage history
                if (_currentIndex < _history.Count - 1)
                    _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));

                _history.Add((canonical, newState));
                _currentIndex++;

                if (!simulation)
                    OnEventPublished?.Invoke(canonical);

                last = canonical;
            }

            return last;
        }

        /// <summary>
        /// Apply a canonical event to produce a new GameState.
        /// Always resolves pieces from the cloned board, not from payload refs.
        /// </summary>
        private GameState ApplyEventToState(GameEvent gameEvent, GameState currentState)
        {
            var newBoard = currentState.Board.Clone();
            var newCurrentPlayer = currentState.CurrentPlayer;
            var newTurnNumber = currentState.TurnNumber;

            switch (gameEvent.Type)
            {
                case GameEventType.MoveApplied:
                    if (gameEvent.Payload is MovePayload movePayload)
                    {
                        var pieceOnBoard = newBoard.GetPieceAt(movePayload.From);
                        if (pieceOnBoard != null)
                            newBoard.MovePiece(movePayload.From, movePayload.To);
                    }
                    break;

                case GameEventType.PieceCaptured:
                    if (gameEvent.Payload is CapturePayload capturePayload)
                    {
                        var pos = capturePayload.Target.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);
                    }
                    break;

                case GameEventType.TurnAdvanced:
                    if (gameEvent.Payload is TurnAdvancedPayload turnPayload)
                    {
                        newCurrentPlayer = turnPayload.NewPlayer;
                        newTurnNumber = turnPayload.TurnNumber;
                    }
                    break;

                case GameEventType.TileEffectTriggered:
                    if (gameEvent.Payload is TileChangePayload tilePayload)
                    {
                        newBoard.SetTile(tilePayload.Position, tilePayload.NewTile);
                    }
                    else if (gameEvent.Payload is ForcedSlidePayload slidePayload)
                    {
                        var p = newBoard.GetPieceAt(slidePayload.From);
                        if (p != null)
                            newBoard.MovePiece(slidePayload.From, slidePayload.To);
                    }
                    break;

                case GameEventType.StatusEffectTriggered:
                    if (gameEvent.Payload is StatusApplyPayload statusPayload)
                    {
                        var targetPos = statusPayload.Target.Position;
                        var pieceAtPos = newBoard.GetPieceAt(targetPos);
                        if (pieceAtPos != null)
                        {
                            if (pieceAtPos is StatusEffectDecorator existing)
                            {
                                existing.AddStatus(statusPayload.Effect);
                            }
                            else
                            {
                                var statusDeco = new StatusEffectDecorator(pieceAtPos);
                                statusDeco.AddStatus(statusPayload.Effect);
                                newBoard.RemovePiece(targetPos);
                                newBoard.PlacePiece(statusDeco, targetPos);
                            }
                        }
                    }
                    else if (gameEvent.Payload is PieceDestroyedPayload destroyedViaStatus)
                    {
                        var pos = destroyedViaStatus.Piece.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);
                    }
                    // StatusTickPayload = no state change
                    break;

                case GameEventType.PieceDestroyed:
                    if (gameEvent.Payload is PieceDestroyedPayload destroyedPayload2)
                    {
                        var pos = destroyedPayload2.Piece.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);
                    }
                    break;

                case GameEventType.StatusTick:
                    // no board mutation
                    break;

                case GameEventType.MoveCancelled:
                    // no board mutation; attacker remains at From
                    break;

                case GameEventType.PiecePromoted:
                    if (gameEvent.Payload is PiecePromotedPayload promo)
                    {
                        var pos = promo.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);

                        newBoard.PlacePiece(promo.NewPiece, pos);
                    }
                    break;
            }

            return new GameState(newBoard, newCurrentPlayer, newTurnNumber);
        }
    }
}
