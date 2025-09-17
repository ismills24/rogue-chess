using ChessRogue.Core.Board;

namespace ChessRogue.Core
{
    public class GameState
    {
        public IBoard Board { get; private set; }
        public PlayerColor CurrentPlayer { get; private set; }
        public int TurnNumber { get; private set; }

        /// <summary>
        /// History of moves applied to reach this state.
        /// Useful for undo, AI training, and debugging.
        /// </summary>
        public IReadOnlyList<Move> MoveHistory => moveHistory.AsReadOnly();
        private readonly List<Move> moveHistory;

        public GameState(IBoard board, PlayerColor startingPlayer = PlayerColor.White)
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
            if (piece == null) return;

            // Destination tile
            var tile = Board.GetTile(move.To);

            // Check tile permission
            if (tile != null && !tile.CanEnter(piece, move.To, this))
                return; // illegal by board rule

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

            // Post-move tile effect
            tile?.OnEnter(piece, move.To, this);

            // Record move
            moveHistory.Add(move);

            // Advance turn
            CurrentPlayer = (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
            TurnNumber++;

            // Trigger "standing on tile" effects for the new current player
            foreach (var standing in Board.GetAllPieces(CurrentPlayer))
            {
                var standingTile = Board.GetTile(standing.Position);
                standingTile?.OnTurnStart(standing, standing.Position, this);
            }
        }

        public void UndoLastMove()
        {
            if (moveHistory.Count == 0) return;

            // Rewind by snapshot instead of trying to “reverse apply”
            var previous = CloneFromHistory(moveHistory.Count - 1);
            this.Board = previous.Board;
            this.CurrentPlayer = previous.CurrentPlayer;
            this.TurnNumber = previous.TurnNumber;
            this.moveHistory.Clear();
            this.moveHistory.AddRange(previous.moveHistory);
        }

        public GameState Clone()
        {
            var clone = new GameState(Board.Clone(), CurrentPlayer) { TurnNumber = TurnNumber };
            clone.moveHistory.AddRange(moveHistory);
            return clone;
        }

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
