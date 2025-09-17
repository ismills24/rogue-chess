using System;
using ChessRogue.Core.Rules;

namespace ChessRogue.Core.Runner
{
    public class GameRunner
    {
        private GameState state;
        private readonly IPlayerController white;
        private readonly IPlayerController black;
        private readonly IWinCondition winCondition;

        public GameRunner(
            GameState initialState,
            IPlayerController whiteController,
            IPlayerController blackController,
            IWinCondition winCondition
        )
        {
            this.state = initialState;
            this.white = whiteController;
            this.black = blackController;
            this.winCondition = winCondition;
        }

        /// <summary>
        /// Runs one turn of the game (asks controller for move, applies it).
        /// </summary>
        public void RunTurn()
        {
            if (winCondition.IsGameOver(state, out var winner))
            {
                Console.WriteLine($"Game Over! Winner: {winner}");
                return;
            }

            IPlayerController controller = state.CurrentPlayer == PlayerColor.White ? white : black;

            var move = controller.SelectMove(state);

            if (move is null)
            {
                Console.WriteLine($"{state.CurrentPlayer} has no legal moves!");
                return;
            }

            state.ApplyMove(move);

            Console.WriteLine($"{state.CurrentPlayer} played {move}");
        }

        public GameState GetState() => state;
    }
}
