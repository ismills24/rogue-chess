using ChessRogue.Core.RuleSets;

namespace ChessRogue.Core.Runner
{
    public class HumanController : IPlayerController
    {
        private readonly IRuleSet ruleSet;

        public HumanController(IRuleSet ruleSet)
        {
            this.ruleSet = ruleSet;
        }

        public Move SelectMove(GameState state)
        {
            var legalMoves = state
                .Board.GetAllPieces(state.CurrentPlayer)
                .SelectMany(p => ruleSet.GetLegalMoves(state, p))
                .ToList();

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
                    Console.WriteLine("Invalid format. Use: e.g., e2 e4");
                    continue;
                }

                var from = Parse(parts[0]);
                var to = Parse(parts[1]);
                if (from == null || to == null)
                {
                    Console.WriteLine("Invalid square.");
                    continue;
                }

                var move = legalMoves.FirstOrDefault(m => m.From == from.Value && m.To == to.Value);
                if (move != null)
                    return move;

                Console.WriteLine("Illegal move. Try again.");
            }
        }

        private Vector2Int? Parse(string s)
        {
            if (s.Length != 2)
                return null;
            var f = s[0];
            var r = s[1];
            if (f < 'a' || f > 'h')
                return null;
            if (r < '1' || r > '8')
                return null;
            return new Vector2Int(f - 'a', r - '1');
        }
    }
}
