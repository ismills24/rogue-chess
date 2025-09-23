using System;
using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// Forces a piece to slide one extra step in its movement direction on entering.
    /// </summary>
    public class SlipperyTile : BaseTile, IInterceptor<MoveEvent>
    {
        public SlipperyTile()
            : base() { }

        public SlipperyTile(Vector2Int pos)
            : base(pos) { }

        public int Priority => 0;

        public IEventSequence Intercept(MoveEvent ev, GameState state)
        {
            if (ev.To != Position)
                return new EventSequence(Array.Empty<GameEvent>(), FallbackPolicy.ContinueChain);

            var dir = new Vector2Int(ev.To.X - ev.From.X, ev.To.Y - ev.From.Y);
            var step = new Vector2Int(Math.Sign(dir.X), Math.Sign(dir.Y));
            var next = ev.To + step;

            if (!state.Board.IsInBounds(next) || state.Board.GetPieceAt(next) != null)
                return new EventSequence(Array.Empty<GameEvent>(), FallbackPolicy.ContinueChain);

            // Only emit the extra slide move
            var slide = new MoveEvent(ev.To, next, ev.Piece, ev.Actor, isPlayerAction: false);
            return new EventSequence(new GameEvent[] { slide }, FallbackPolicy.ContinueChain);
        }

        public override ITile Clone() => new SlipperyTile(Position);
    }
}
