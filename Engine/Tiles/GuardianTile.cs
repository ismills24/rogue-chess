using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// A GuardianTile protects the piece standing on it once.
    /// - If a capture targets the occupant, the capture is cancelled,
    ///   the tile is consumed (replaced with Standard), and the turn ends.
    /// - If a move would land onto this tile while occupied, the move is cancelled,
    ///   the tile is consumed, and the turn ends.
    /// After triggering once, the tile becomes a StandardTile.
    /// </summary>
    public class GuardianTile : BaseTile, IBeforeEventHook
    {
        public GuardianTile()
            : base() { }

        public GuardianTile(Vector2Int position)
            : base(position) { }

        public override IEnumerable<CandidateEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield break;
        }

        public override IEnumerable<CandidateEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield break;
        }

        public IEnumerable<CandidateEvent>? BeforeEvent(CandidateEvent candidate, GameState state)
        {
            // Protect occupant from being captured
            if (
                candidate.Type == GameEventType.PieceCaptured
                && candidate.Payload is CapturePayload cap
                && cap.Target.Position == Position
            )
            {
                var nextPlayer =
                    state.CurrentPlayer == PlayerColor.White
                        ? PlayerColor.Black
                        : PlayerColor.White;
                var nextTurn = state.TurnNumber + 1;

                return new[]
                {
                    // Consume the tile
                    new CandidateEvent(
                        GameEventType.TileEffectTriggered,
                        false,
                        new TileChangePayload(Position, new StandardTile(Position))
                    ),
                    // End the turn immediately (capture prevented)
                    new CandidateEvent(
                        GameEventType.TurnAdvanced,
                        false,
                        new TurnAdvancedPayload(nextPlayer, nextTurn)
                    ),
                };
            }

            // Cancel moves landing onto this tile if it's occupied
            if (
                candidate.Type == GameEventType.MoveApplied
                && candidate.Payload is MovePayload move
                && move.To == Position
            )
            {
                var occupant = state.Board.GetPieceAt(Position);
                if (occupant != null)
                {
                    var nextPlayer =
                        state.CurrentPlayer == PlayerColor.White
                            ? PlayerColor.Black
                            : PlayerColor.White;
                    var nextTurn = state.TurnNumber + 1;

                    return new[]
                    {
                        // Consume the tile
                        new CandidateEvent(
                            GameEventType.TileEffectTriggered,
                            false,
                            new TileChangePayload(Position, new StandardTile(Position))
                        ),
                        // End the turn; move cancelled
                        new CandidateEvent(
                            GameEventType.TurnAdvanced,
                            false,
                            new TurnAdvancedPayload(nextPlayer, nextTurn)
                        ),
                    };
                }
            }

            // Default: pass through unchanged
            return new[] { candidate };
        }

        public override ITile Clone() => new GuardianTile(Position);
    }
}
