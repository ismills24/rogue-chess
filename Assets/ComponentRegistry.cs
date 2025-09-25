using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.Tiles;

namespace RogueChess.Assets
{
    /// <summary>
    /// Central registry of known components (pieces, tiles, decorators).
    /// Provides both construction and asset lookup.
    /// </summary>
    public static class ComponentRegistry
    {
        // --- Factories for engine objects ---
        public static string AssetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        private static readonly Dictionary<string, Func<PlayerColor, IPiece>> PieceFactories = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "Pawn", color => new Pawn(color, new Vector2Int(0, 0)) },
            { "Rook", color => new Rook(color, new Vector2Int(0, 0)) },
            { "Knight", color => new Knight(color, new Vector2Int(0, 0)) },
            { "Bishop", color => new Bishop(color, new Vector2Int(0, 0)) },
            { "Queen", color => new Queen(color, new Vector2Int(0, 0)) },
            { "King", color => new King(color, new Vector2Int(0, 0)) },
        };

        private static readonly Dictionary<string, Func<IPiece, IPiece>> DecoratorFactories = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "MarksmanDecorator", inner => new MarksmanDecorator(inner) },
            { "ExplodingDecorator", inner => new ExplodingDecorator(inner) },
            { "ScapegoatDecorator", inner => new ScapegoatDecorator(inner) },
        };

        private static readonly Dictionary<string, Func<ITile>> TileFactories = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "StandardTile", () => new StandardTile() },
            { "SlipperyTile", () => new SlipperyTile() },
            { "GuardianTile", () => new GuardianTile() },
        };

        // --- Image lookup tables ---
        // pieces: key = (type, color)
        private static readonly Dictionary<(string Type, PlayerColor Color), string> PieceImages =
            new()
            {
                { ("Pawn", PlayerColor.White), Path.Combine(AssetsPath, "Pieces", "pawn-w.svg") },
                { ("Pawn", PlayerColor.Black), Path.Combine(AssetsPath, "Pieces", "pawn-b.svg") },
                { ("Rook", PlayerColor.White), Path.Combine(AssetsPath, "Pieces", "rook-w.svg") },
                { ("Rook", PlayerColor.Black), Path.Combine(AssetsPath, "Pieces", "rook-b.svg") },
                {
                    ("Knight", PlayerColor.White),
                    Path.Combine(AssetsPath, "Pieces", "knight-w.svg")
                },
                {
                    ("Knight", PlayerColor.Black),
                    Path.Combine(AssetsPath, "Pieces", "knight-b.svg")
                },
                {
                    ("Bishop", PlayerColor.White),
                    Path.Combine(AssetsPath, "Pieces", "bishop-w.svg")
                },
                {
                    ("Bishop", PlayerColor.Black),
                    Path.Combine(AssetsPath, "Pieces", "bishop-b.svg")
                },
                { ("Queen", PlayerColor.White), Path.Combine(AssetsPath, "Pieces", "queen-w.svg") },
                { ("Queen", PlayerColor.Black), Path.Combine(AssetsPath, "Pieces", "queen-b.svg") },
                { ("King", PlayerColor.White), Path.Combine(AssetsPath, "Pieces", "king-w.svg") },
                { ("King", PlayerColor.Black), Path.Combine(AssetsPath, "Pieces", "king-b.svg") },
            };

        private static readonly Dictionary<string, string> DecoratorImages = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "MarksmanDecorator", Path.Combine(AssetsPath, "Decorators", "marksman.svg") },
            { "ExplodingDecorator", Path.Combine(AssetsPath, "Decorators", "exploding.svg") },
            { "ScapegoatDecorator", Path.Combine(AssetsPath, "Decorators", "scapegoat.svg") },
        };

        private static readonly Dictionary<string, string> TileImages = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "StandardTile", Path.Combine(AssetsPath, "Tiles", "standard.svg") },
            { "SlipperyTile", Path.Combine(AssetsPath, "Tiles", "slippery.svg") },
            { "GuardianTile", Path.Combine(AssetsPath, "Tiles", "guardian.svg") },
        };

        // --- Public catalogs (sorted for stable UI order) ---

        public static IReadOnlyList<string> GetPieceTypes() =>
            PieceFactories.Keys.OrderBy(k => k).ToList();

        public static IReadOnlyList<string> GetTileTypes() =>
            TileFactories.Keys.OrderBy(k => k).ToList();

        public static IReadOnlyList<string> GetDecoratorTypes() =>
            DecoratorFactories.Keys.OrderBy(k => k).ToList();

        public static bool IsPieceType(string type) => PieceFactories.ContainsKey(type);

        public static bool IsTileType(string type) => TileFactories.ContainsKey(type);

        public static bool IsDecoratorType(string type) => DecoratorFactories.ContainsKey(type);

        // --- Construction helpers ---

        public static IPiece CreatePiece(string type, PlayerColor color)
        {
            if (!PieceFactories.TryGetValue(type, out var factory))
                throw new ArgumentException($"Unknown piece type: {type}");
            return factory(color);
        }

        public static IPiece ApplyDecorator(string type, IPiece inner)
        {
            if (!DecoratorFactories.TryGetValue(type, out var factory))
                throw new ArgumentException($"Unknown decorator type: {type}");
            return factory(inner);
        }

        public static ITile CreateTile(string type)
        {
            if (!TileFactories.TryGetValue(type, out var factory))
                throw new ArgumentException($"Unknown tile type: {type}");
            return factory();
        }

        // --- Image helpers ---

        public static string GetPieceImagePath(string type, PlayerColor color) =>
            PieceImages.TryGetValue((type, color), out var path)
                ? path
                : Path.Combine(AssetsPath, "Pieces", "unknown.svg");

        public static string GetDecoratorImagePath(string type) =>
            DecoratorImages.TryGetValue(type, out var path)
                ? path
                : Path.Combine(AssetsPath, "Decorators", "unknown.svg");

        public static string GetTileImagePath(string type) =>
            TileImages.TryGetValue(type, out var path)
                ? path
                : Path.Combine(AssetsPath, "Tiles", "unknown.svg");
    }
}
