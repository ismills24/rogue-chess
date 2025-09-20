using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Board
{
    /// <summary>
    /// Tile that provides protection to pieces standing on it.
    /// When a piece on this tile is about to be captured, the tile absorbs the damage instead.
    /// </summary>
    public class GuardianTile : ITile, IBeforeEventHook
    {
        public Vector2Int Position { get; set; }

        public IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state)
        {
            yield break; // No effects on enter
        }

        public IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state)
        {
            yield break; // No effects on turn start
        }

        public CandidateEvent? BeforeEvent(CandidateEvent candidate, GameState state)
        {
            // Only intercept capture events
            if (candidate.Type != GameEventType.PieceCaptured)
                return candidate;

            // Check if the capture payload contains a piece on this tile
            if (candidate.Payload is not CapturePayload capturePayload)
                return candidate;

            var targetPiece = capturePayload.Target;
            
            // Only protect pieces on this tile
            if (targetPiece.Position != Position)
                return candidate;

            // Cancel the capture - the tile absorbs the damage
            return null;
        }

        public ITile Clone()
        {
            return new GuardianTile { Position = Position };
        }
    }
}
