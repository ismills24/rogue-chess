using System;
using System.Linq;

namespace ChessRogue.Core.Runner
{
    public class RandomAIController : IPlayerController
    {
        private readonly Random rng = new Random();

        public Move SelectMove(GameState state)
        {
            var pieces = state.Board.GetAllPieces(state.CurrentPlayer);
            var legalMoves = pieces.SelectMany(p => p.GetLegalMoves(state)).ToList();

            if (legalMoves.Count == 0)
                return null;

            return legalMoves[rng.Next(legalMoves.Count)];
        }
    }
}
