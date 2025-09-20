using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine
{
    /// <summary>
    /// Immutable snapshot of the current game state.
    /// Each GameState represents a single moment in time.
    /// </summary>
    public class GameState
    {
        public IBoard Board { get; }
        public PlayerColor CurrentPlayer { get; }
        public int TurnNumber { get; }
        public IReadOnlyList<Move> MoveHistory { get; }

        private readonly List<Move> moveHistory;

        public GameState(IBoard board, PlayerColor currentPlayer, int turnNumber = 1)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            CurrentPlayer = currentPlayer;
            TurnNumber = turnNumber;
            moveHistory = new List<Move>();
            MoveHistory = moveHistory.AsReadOnly();
        }

        private GameState(
            IBoard board,
            PlayerColor currentPlayer,
            int turnNumber,
            List<Move> moveHistory
        )
        {
            Board = board;
            CurrentPlayer = currentPlayer;
            TurnNumber = turnNumber;
            this.moveHistory = new List<Move>(moveHistory);
            MoveHistory = this.moveHistory.AsReadOnly();
        }

        /// <summary>
        /// Create a deep clone of this GameState.
        /// Changes to the clone will not affect the original.
        /// </summary>
        public GameState Clone()
        {
            return new GameState(Board.Clone(), CurrentPlayer, TurnNumber, moveHistory);
        }

        /// <summary>
        /// Create the initial game state with a board and starting player.
        /// </summary>
        public static GameState CreateInitial(IBoard board, PlayerColor startPlayer)
        {
            return new GameState(board, startPlayer, 1);
        }

        /// <summary>
        /// Create a new GameState with updated properties.
        /// This is used internally by the GameEngine during state transitions.
        /// </summary>
        internal GameState WithUpdatedState(
            IBoard? board = null,
            PlayerColor? currentPlayer = null,
            int? turnNumber = null,
            Move? additionalMove = null
        )
        {
            var newBoard = board ?? Board.Clone();
            var newCurrentPlayer = currentPlayer ?? CurrentPlayer;
            var newTurnNumber = turnNumber ?? TurnNumber;

            var newMoveHistory = new List<Move>(moveHistory);
            if (additionalMove != null)
            {
                newMoveHistory.Add(additionalMove);
            }

            return new GameState(newBoard, newCurrentPlayer, newTurnNumber, newMoveHistory);
        }

        /// <summary>
        /// Simulate a move and return the resulting GameState.
        /// This is for AI evaluation and does NOT affect the canonical game history.
        /// The returned state is a deep clone that can be safely modified.
        /// </summary>
        /// <param name="move">The move to simulate</param>
        /// <param name="ruleset">Ruleset to use for legal move validation</param>
        /// <returns>New GameState representing the position after the move, or null if move is illegal</returns>
        public GameState? Simulate(Move move, IRuleSet ruleset)
        {
            // Check if the move is legal
            var piece = Board.GetPieceAt(move.From);
            if (piece == null)
                return null;

            var legalMoves = ruleset.GetLegalMoves(this, piece);
            if (!legalMoves.Contains(move))
                return null;

            // Create a deep clone for simulation
            var simulatedState = Clone();
            var simulatedBoard = simulatedState.Board;

            // Find the corresponding piece in the cloned board
            var simulatedPiece = simulatedBoard.GetPieceAt(move.From);
            if (simulatedPiece == null)
                return null;

            // Apply the move to the simulated state
            var capturedPiece = simulatedBoard.GetPieceAt(move.To);
            if (capturedPiece != null)
            {
                simulatedBoard.RemovePiece(move.To);
            }

            simulatedBoard.MovePiece(move.From, move.To);
            simulatedPiece.Position = move.To;

            // Create new state with the move added to history
            var newMoveHistory = new List<Move>(moveHistory) { move };
            return new GameState(simulatedBoard, CurrentPlayer, TurnNumber, newMoveHistory);
        }

        /// <summary>
        /// Evaluate the current position for AI purposes.
        /// Returns a numerical score where positive values favor White and negative values favor Black.
        /// </summary>
        /// <returns>Position evaluation score</returns>
        public int Evaluate()
        {
            int score = 0;

            for (int x = 0; x < Board.Width; x++)
            {
                for (int y = 0; y < Board.Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var piece = Board.GetPieceAt(pos);
                    if (piece == null)
                        continue;

                    var v = PieceValueCalculator.GetTotalValue(piece); // <-- includes decorators/status
                    score += piece.Owner == PlayerColor.White ? v : -v;
                }
            }

            return score; // + = good for White, - = good for Black
        }

        /// <summary>
        /// Get all legal moves for the current player.
        /// This is a convenience method for AI evaluation.
        /// </summary>
        /// <param name="ruleset">Ruleset to use for move validation</param>
        /// <returns>All legal moves for the current player</returns>
        public IEnumerable<Move> GetAllLegalMoves(IRuleSet ruleset)
        {
            var currentPlayerPieces = Board.GetAllPieces(CurrentPlayer);
            foreach (var piece in currentPlayerPieces)
            {
                foreach (var move in ruleset.GetLegalMoves(this, piece))
                {
                    yield return move;
                }
            }
        }
    }
}
