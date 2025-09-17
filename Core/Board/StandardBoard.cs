namespace ChessRogue.Core.Board
{
    public class StandardBoard : BoardBase
    {
        public StandardBoard(int width = 8, int height = 8)
            : base(width, height)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tiles[x, y] = new StandardTile();
        }

        protected override BoardBase CreateEmpty(int width, int height) =>
            new StandardBoard(width, height);
    }
}
