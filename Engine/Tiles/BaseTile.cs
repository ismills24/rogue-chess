using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// Base implementation of ITile with common functionality.
    /// Most tiles can inherit from this and override specific methods.
    /// </summary>
    public abstract class BaseTile : ITile
    {
        public Vector2Int Position { get; set; }

        protected BaseTile() => Position = new Vector2Int(0, 0);

        protected BaseTile(Vector2Int position) => Position = position;

        public virtual IEnumerable<CandidateEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield break;
        }

        public virtual IEnumerable<CandidateEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield break;
        }

        public abstract ITile Clone();
    }
}


