using ChessRogue.Core.Events;
using ChessRogue.Core.RuleSets;
using ChessRogue.Core.StatusEffects;

namespace ChessRogue.Core.Runner
{
    public class GameRunner
    {
        private GameState state;
        private readonly IPlayerController white;
        private readonly IPlayerController black;
        private readonly IRuleSet ruleset;
        private bool gameOver;
        public bool IsGameOver => gameOver;

        public GameState GetState() => state;

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

        public event Action<GameEvent>? OnEventPublished;

        public void RunTurn()
        {
            if (gameOver)
                return;

            // --- Pre-move win condition check ---
            if (ruleset.IsGameOver(state, out var preWinner))
            {
                EndGame(preWinner);
                return;
            }

            // --- Tick status effects at start of turn ---
            foreach (var piece in state.Board.GetAllPieces(state.CurrentPlayer).ToList())
            {
                if (piece is IStatusEffectCarrier carrier)
                {
                    foreach (var status in carrier.GetStatuses().ToList())
                    {
                        foreach (var ev in status.OnTurnStart(piece, state))
                            Publish(ev);
                    }
                }

                var tile = state.Board.GetTile(piece.Position);
                if (tile != null)
                {
                    foreach (var ev in tile.OnTurnStart(piece, piece.Position, state))
                        Publish(ev);
                }
            }

            // --- Gather legal moves ---
            IPlayerController controller = state.CurrentPlayer == PlayerColor.White ? white : black;

            var pieces = state.Board.GetAllPieces(state.CurrentPlayer);
            var legalMoves = pieces.SelectMany(p => ruleset.GetLegalMoves(state, p)).ToList();

            if (legalMoves.Count == 0)
            {
                var winner =
                    state.CurrentPlayer == PlayerColor.White
                        ? PlayerColor.Black
                        : PlayerColor.White;
                EndGame(winner);
                return;
            }

            // --- Get chosen move ---
            var move = controller.SelectMove(state);
            if (move is null)
            {
                var winner =
                    state.CurrentPlayer == PlayerColor.White
                        ? PlayerColor.Black
                        : PlayerColor.White;
                EndGame(winner);
                return;
            }

            // --- Apply move and publish resulting events ---
            var events = state.ApplyMove(move);
            foreach (var e in events)
                Publish(e);

            // --- Post-move win condition check ---
            if (ruleset.IsGameOver(state, out var postWinner))
            {
                EndGame(postWinner);
            }
        }

        private void EndGame(PlayerColor winner)
        {
            Publish(
                new GameEvent(
                    GameEventType.GameOver,
                    null,
                    null,
                    null,
                    winner == default ? "Stalemate" : $"{winner} wins!"
                )
            );
            gameOver = true;
        }

        // GameRunner.cs (Publish)
        private void Publish(GameEvent e)
        {
            // Handle real side effects for tile-driven forced movement
            if (
                e.Type == GameEventType.TileEffectTriggered
                && e.Piece != null
                && e.From != null
                && e.To != null
            )
            {
                // Guard: ignore no-op moves (From == To)
                if (e.From.Value != e.To.Value)
                {
                    // Move the piece
                    state.Board.MovePiece(e.From.Value, e.To.Value);
                    e.Piece.Position = e.To.Value;
                    state.RecordSyntheticMove(new Move(e.From.Value, e.To.Value, e.Piece, false));

                    // Chain: trigger tile effects at the new square
                    var tile = state.Board.GetTile(e.To.Value);
                    if (tile != null)
                    {
                        foreach (var ev in tile.OnEnter(e.Piece, e.To.Value, state))
                            Publish(ev); // recursive chain, but safe due to From!=To guard & tiles not mutating
                    }
                }
            }

            OnEventPublished?.Invoke(e);
        }
    }
}
