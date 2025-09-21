using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// Applies Burning to pieces that enter or start on it.
    /// </summary>
    public class ScorchedTile : BaseTile
    {
        public ScorchedTile()
            : base() { }

        public ScorchedTile(Vector2Int position)
            : base(position) { }

        public override IEnumerable<CandidateEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield return new CandidateEvent(
                GameEventType.StatusEffectTriggered,
                false,
                new StatusApplyPayload(piece, new BurningStatus())
            );
        }

        public override IEnumerable<CandidateEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield return new CandidateEvent(
                GameEventType.StatusEffectTriggered,
                false,
                new StatusApplyPayload(piece, new BurningStatus())
            );
        }

        public override ITile Clone() => new ScorchedTile(Position);
    }
}


