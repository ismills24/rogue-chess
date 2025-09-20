namespace RogueChess.Engine.Primitives
{
    /// <summary>
    /// Immutable 2D integer vector, used for board coordinates.
    /// (0,0) is the bottom-left of the board.
    /// </summary>
    public readonly struct Vector2Int
    {
        public int X { get; }
        public int Y { get; }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static Vector2Int operator -(Vector2Int a, Vector2Int b) =>
            new(a.X - b.X, a.Y - b.Y);

        public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);

        public override bool Equals(object? obj) => obj is Vector2Int other && this == other;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X},{Y})";
    }
}
