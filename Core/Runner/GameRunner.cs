using System;
using ChessRogue.Core.RuleSets;

namespace ChessRogue.Core.Runner
{
    public class GameRunner
    {
        private GameState state;
        private readonly IPlayerController white;
        private readonly IPlayerController black;
        private readonly IRuleSet ruleset;
        private bool gameOver;

        public GameRunner(
            GameState initialState,
            IPlayerController whiteController,
            IPlayerController blackController,
            IRuleSet ruleset
        )
        {
            this.state = initialState;
            this.white = whiteController;
            this.black = blackController;
            this.ruleset = ruleset;
            this.gameOver = false;
        }

        public void RunTurn()
        {
            if (gameOver)
                return;

            if (ruleset.IsGameOver(state, out var preWinner))
            {
                AnnounceGameOver(preWinner);
                return;
            }

            IPlayerController controller = state.CurrentPlayer == PlayerColor.White ? white : black;
            var pieces = state.Board.GetAllPieces(state.CurrentPlayer);
            var legalMoves = pieces.SelectMany(p => ruleset.GetLegalMoves(state, p)).ToList();

            if (legalMoves.Count == 0)
            {
                AnnounceGameOver(
                    state.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White
                );
                return;
            }

            var move = controller.SelectMove(state);

            if (move is null)
            {
                AnnounceGameOver(
                    state.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White
                );
                return;
            }

            var movingSide = state.CurrentPlayer;
            state.ApplyMove(move);
            Console.WriteLine($"{movingSide} played {move}");

            if (ruleset.IsGameOver(state, out var postWinner))
            {
                AnnounceGameOver(postWinner);
            }
        }

        private void AnnounceGameOver(PlayerColor winner)
        {
            gameOver = true;

            if (winner == default)
                Console.WriteLine("Game over — stalemate. It's a draw!");
            else
                Console.WriteLine($"Game over — {winner} wins!");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public GameState GetState() => state;
    }
}
