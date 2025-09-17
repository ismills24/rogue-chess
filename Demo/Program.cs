using ChessRogue.Core;
using ChessRogue.Core.Board;
using ChessRogue.Core.Board.Tiles;
using ChessRogue.Core.Pieces.Decorators;
using ChessRogue.Core.RuleSets;
using ChessRogue.Core.Runner;
using ChessRogue.Core.StatusEffects;

class Program
{
    static void Main()
    {
        Console.WriteLine("Choose game mode:");
        Console.WriteLine("1. Standard Chess");
        Console.WriteLine("2. Custom: Last Piece Standing");
        Console.Write("Enter choice: ");
        var choice = Console.ReadLine();

        (GameState state, IRuleSet ruleSet) setup;

        if (choice == "2")
            setup = CustomGameSetup.Create();
        else
            setup = StandardGameSetup.Create();

        var runner = new GameRunner(
            setup.state,
            new HumanController(setup.ruleSet), // White
            new HumanController(setup.ruleSet), // Black
            setup.ruleSet
        );

        while (!runner.IsGameOver)
        {
            PrintBoard(setup.state.Board);
            runner.RunTurn();
        }

        PrintBoard(setup.state.Board);
        Console.WriteLine("Game over. Press any key to exit...");
        Console.ReadKey();
    }

    static void PrintBoard(IBoard board)
    {
        Console.WriteLine();
        Console.WriteLine("    a b c d e f g h");
        Console.WriteLine("   -----------------");

        for (int y = board.Height - 1; y >= 0; y--)
        {
            Console.Write($"{y + 1} | ");

            for (int x = 0; x < board.Width; x++)
            {
                var pos = new Vector2Int(x, y);
                var symbol = GetBoardSymbol(board, pos);
                Console.Write(symbol + " ");
            }

            Console.WriteLine($"| {y + 1}");
        }

        Console.WriteLine("   -----------------");
        Console.WriteLine("    a b c d e f g h");
        Console.WriteLine();
    }

    static char GetBoardSymbol(IBoard board, Vector2Int pos)
    {
        var piece = board.GetPieceAt(pos);
        var tile = board.GetTile(pos);

        if (piece == null)
        {
            // --- Tile overlays ---
            if (tile is SlipperyTile)
                return '#';
            if (tile is ScorchedTile)
                return '*';
            return '.';
        }

        // --- Piece rendering ---
        char c = piece.Name[0];

        // Lowercase for black
        if (piece.Owner == PlayerColor.Black)
            c = char.ToLower(c);

        // Exploding decorator overrides symbol
        if (piece is ExplodingPieceDecorator)
            return '!';

        // Burning status overrides symbol
        if (
            piece is IStatusEffectCarrier carrier
            && carrier.GetStatuses().Any(s => s.Name == "Burning")
        )
            return '\'';

        return c;
    }
}
