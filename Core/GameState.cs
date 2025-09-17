namespace ChessRogue.Core
{
    public class GameState
    {
        public Board Board { get; private set; }
        public PlayerColor CurrentPlayer { get; private set; }
        public int TurnNumber { get; private set; }

        /// <summary>
        /// History of moves applied to reach this state.
        /// Useful for undo, AI training, and debugging.
        /// </summary>
        public IReadOnlyList<Move> MoveHistory => moveHistory.AsReadOnly();
        private readonly List<Move> moveHistory;

        public GameState(Board board, PlayerColor startingPlayer = PlayerColor.White)
        {
            Board = board;
            CurrentPlayer = startingPlayer;
            TurnNumber = 1;
            moveHistory = new List<Move>();
        }

        /// <summary>
        /// Apply a move and advance the state.
        /// Note: does not validate legality — caller must check first.
        /// </summary>
        public void ApplyMove(Move move)
        {
            var piece = Board.GetPieceAt(move.From);
            if (piece == null)
                return;

            // Handle captures
            var captured = Board.GetPieceAt(move.To);
            if (captured != null)
            {
                Board.RemovePiece(move.To);
                captured.OnCapture(this);
            }

            // Move piece
            Board.MovePiece(move.From, move.To);
            piece.OnMove(move, this);

            // Record move
            moveHistory.Add(move);

            // Advance turn
            CurrentPlayer =
                (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
            TurnNumber++;
        }

        /// <summary>
        /// Undo the last move by restoring from a previous snapshot.
        /// (Shallow: assumes pieces are immutable except position).
        /// </summary>
        public void UndoLastMove()
        {
            if (moveHistory.Count == 0)
                return;

            // Rewind by snapshot instead of trying to “reverse apply”
            var previous = CloneFromHistory(moveHistory.Count - 1);
            this.Board = previous.Board;
            this.CurrentPlayer = previous.CurrentPlayer;
            this.TurnNumber = previous.TurnNumber;
            this.moveHistory.Clear();
            this.moveHistory.AddRange(previous.moveHistory);
        }

        /// <summary>
        /// Creates a deep snapshot of the state for undo/AI simulation.
        /// Note: pieces are shallow-copied unless IPiece.Clone() is implemented.
        /// </summary>
        public GameState Clone()
        {
            var clone = new GameState(Board.Clone(), CurrentPlayer) { TurnNumber = TurnNumber };
            clone.moveHistory.AddRange(moveHistory);
            return clone;
        }

        /// <summary>
        /// Returns a snapshot from history N moves deep.
        /// (Re-applies moves from the beginning on a cloned board).
        /// </summary>
        private GameState CloneFromHistory(int moveCount)
        {
            var clone = new GameState(Board.Clone(), PlayerColor.White);
            for (int i = 0; i < moveCount; i++)
            {
                clone.ApplyMove(moveHistory[i]);
            }
            return clone;
        }
    }
}
