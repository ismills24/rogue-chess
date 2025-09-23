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

        public abstract ITile Clone();
    }
}
