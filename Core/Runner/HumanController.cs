using System;
using System.Linq;

namespace ChessRogue.Core.Runner
{
    public class HumanController : IPlayerController
    {
        public Move SelectMove(GameState state)
        {
            // Dump all legal moves
            var pieces = state.Board.GetAllPieces(state.CurrentPlayer);
            var legalMoves = pieces.SelectMany(p => p.GetLegalMoves(state)).ToList();

            if (legalMoves.Count == 0)
                return null;

            Console.WriteLine($"{state.CurrentPlayer}'s turn. Legal moves:");
            for (int i = 0; i < legalMoves.Count; i++)
            {
                Console.WriteLine($"{i}: {legalMoves[i]}");
            }

            Console.Write("Select move index: ");
            var input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice >= 0 && choice < legalMoves.Count)
                return legalMoves[choice];

            Console.WriteLine("Invalid input, selecting first legal move by default.");
            return legalMoves[0];
        }
    }
}
