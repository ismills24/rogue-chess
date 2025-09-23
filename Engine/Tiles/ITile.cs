using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    public interface ITile
    {
        Vector2Int Position { get; set; }
        ITile Clone();
    }
}
