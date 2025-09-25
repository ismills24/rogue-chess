using System.Collections.Generic;

namespace RogueChess.Engine.Maps
{
    /// <summary>
    /// Root definition of a map. Serializable to/from JSON.
    /// </summary>
    public class MapDefinition
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public List<TileDefinition> Tiles { get; set; } = new List<TileDefinition>();
        public List<PieceDefinition> Pieces { get; set; } = new List<PieceDefinition>();
    }

    public class TileDefinition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } // e.g. "StandardTile", "SlipperyTile"
    }

    public class PieceDefinition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } // e.g. "Pawn", "Rook"
        public string Owner { get; set; } // "White" or "Black"
        public List<DecoratorDefinition> Decorators { get; set; } = new List<DecoratorDefinition>();
    }

    public class DecoratorDefinition
    {
        public string Type { get; set; } // e.g. "MarksmanDecorator", "ExplodingDecorator"
    }
}
