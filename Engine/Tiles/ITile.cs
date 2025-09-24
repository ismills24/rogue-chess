using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    public interface ITile
    {
        Vector2Int Position { get; set; }
        System.Guid ID { get; }
        ITile Clone();
    }
}
