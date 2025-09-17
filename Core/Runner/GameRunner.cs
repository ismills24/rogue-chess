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
        private bool gameOver;

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
            this.gameOver = false;
        }

        public void RunTurn()
        {
            if (gameOver)
                return;

            // Pre-move check (stalemate, no-legal-moves before a move is made)
            if (winCondition.IsGameOver(state, out var preWinner))
            {
                AnnounceGameOver(preWinner);
                return;
            }

            var movingSide = state.CurrentPlayer;
            IPlayerController controller = movingSide == PlayerColor.White ? white : black;

            var move = controller.SelectMove(state);

            if (move is null)
            {
                Console.WriteLine($"{movingSide} has no legal moves!");
                AnnounceGameOver(
                    movingSide == PlayerColor.White ? PlayerColor.Black : PlayerColor.White
                );
                return;
            }

            state.ApplyMove(move);
            Console.WriteLine($"{movingSide} played {move}");

            // 🔑 Checkmate/stalemate *after* this move
            if (winCondition.IsGameOver(state, out var postWinner))
            {
                AnnounceGameOver(postWinner);
            }
        }

        private void AnnounceGameOver(PlayerColor winner)
        {
            gameOver = true;

            if (winner == default)
            {
                Console.WriteLine("Game over — stalemate. It's a draw!");
            }
            else
            {
                Console.WriteLine($"Game over — {winner} wins!");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public GameState GetState() => state;
    }
}
