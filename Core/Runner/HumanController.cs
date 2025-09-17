using ChessRogue.Core.RuleSets;

namespace ChessRogue.Core.Runner
{
    public class HumanController : IPlayerController
    {
        private readonly IRuleSet ruleset;

        public HumanController(IRuleSet ruleset)
        {
            this.ruleset = ruleset;
        }

        public Move SelectMove(GameState state)
        {
            var pieces = state.Board.GetAllPieces(state.CurrentPlayer);
            var pseudoMoves = pieces.SelectMany(p => p.GetPseudoLegalMoves(state)).ToList();
            Console.WriteLine(
                $"DEBUG: {pseudoMoves.Count} pseudo-legal moves for {state.CurrentPlayer}"
            );
            var legalMoves = pieces.SelectMany(p => ruleset.GetLegalMoves(state, p)).ToList();

            if (legalMoves.Count == 0)
                return null;

            while (true)
            {
                Console.WriteLine($"{state.CurrentPlayer}'s turn. Enter move (e.g., e2 e4): ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid format. Use: e2 e4");
                    continue;
                }

                var from = ParseSquare(parts[0]);
                var to = ParseSquare(parts[1]);

                if (from == null || to == null)
                {
                    Console.WriteLine("Invalid square. Use a-h and 1-8.");
                    continue;
                }

                var move = legalMoves.FirstOrDefault(m => m.From == from.Value && m.To == to.Value);
                if (move != null)
                    return move;

                Console.WriteLine("Illegal move. Try again.");
            }
        }

        private Vector2Int? ParseSquare(string algebraic)
        {
            if (algebraic.Length != 2)
                return null;

            char file = algebraic[0];
            char rank = algebraic[1];

            if (file < 'a' || file > 'h')
                return null;
            if (rank < '1' || rank > '8')
                return null;

            int x = file - 'a';
            int y = rank - '1';
            return new Vector2Int(x, y);
        }
    }
}
