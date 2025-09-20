using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// A standard tile with no special effects.
    /// This is the default tile type for most board positions.
    /// </summary>
    public class StandardTile : BaseTile
    {
        public StandardTile()
            : base() { }

        public StandardTile(Vector2Int position)
            : base(position) { }

        public override ITile Clone()
        {
            return new StandardTile(Position);
        }
    }
}
