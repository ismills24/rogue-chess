using ChessRogue.Core.Board;
using ChessRogue.Core.Events;

namespace ChessRogue.Core
{
    public class GameState
    {
        public IBoard Board { get; private set; }
        public PlayerColor CurrentPlayer { get; private set; }
        public int TurnNumber { get; private set; }

        public IReadOnlyList<Move> MoveHistory => moveHistory.AsReadOnly();
        private readonly List<Move> moveHistory;

        /// <summary>
        /// Central queue of events emitted during gameplay.
        /// The runner / frontend drains this queue in order.
        /// </summary>
        private readonly Queue<GameEvent> eventQueue = new();

        public GameState(IBoard board, PlayerColor startingPlayer = PlayerColor.White)
        {
            Board = board;
            CurrentPlayer = startingPlayer;
            TurnNumber = 1;
            moveHistory = new List<Move>();
        }

        // ------------------------------
        // Event management
        // ------------------------------

        public void EnqueueEvent(GameEvent ev) => eventQueue.Enqueue(ev);

        public bool HasPendingEvents => eventQueue.Count > 0;

        public GameEvent DequeueEvent() => eventQueue.Count > 0 ? eventQueue.Dequeue() : null;

        public IEnumerable<GameEvent> DrainEvents()
        {
            while (eventQueue.Count > 0)
                yield return eventQueue.Dequeue();
        }

        // ------------------------------
        // Core state mutations
        // ------------------------------

        /// <summary>
        /// Apply a move and enqueue resulting events.
        /// </summary>
        public IReadOnlyList<GameEvent> ApplyMove(Move move)
        {
            var events = new List<GameEvent>();
            var piece = Board.GetPieceAt(move.From);
            if (piece == null)
                return events;

            // Captures
            var captured = Board.GetPieceAt(move.To);
            if (captured != null)
            {
                Board.RemovePiece(move.To);
                captured.OnCapture(this);
                events.Add(
                    new GameEvent(GameEventType.PieceCaptured, captured, move.From, move.To)
                );
            }

            // Move the piece
            Board.MovePiece(move.From, move.To);
            piece.OnMove(move, this);
            events.Add(new GameEvent(GameEventType.MoveApplied, piece, move.From, move.To));

            // Promotion
            if (
                piece is Pawn pawn
                && (
                    (pawn.Owner == PlayerColor.White && move.To.y == 7)
                    || (pawn.Owner == PlayerColor.Black && move.To.y == 0)
                )
            )
            {
                var promoted = new Queen(pawn.Owner, move.To);
                Board.PlacePiece(promoted, move.To);
                events.Add(
                    new GameEvent(GameEventType.PiecePromoted, promoted, move.From, move.To)
                );
            }

            // Record move
            moveHistory.Add(move);
            CurrentPlayer =
                (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
            TurnNumber++;
            events.Add(
                new GameEvent(GameEventType.TurnAdvanced, null, null, null, $"Turn {TurnNumber}")
            );

            // NEW: trigger tile entry for the destination square
            var landedTile = Board.GetTile(move.To);
            if (landedTile != null)
            {
                foreach (var ev in landedTile.OnEnter(piece, move.To, this))
                    events.Add(ev);
            }

            return events;
        }

        public void UndoLastMove()
        {
            if (moveHistory.Count == 0)
                return;

            var previous = CloneFromHistory(moveHistory.Count - 1);
            Board = previous.Board;
            CurrentPlayer = previous.CurrentPlayer;
            TurnNumber = previous.TurnNumber;
            moveHistory.Clear();
            moveHistory.AddRange(previous.moveHistory);
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
                clone.DrainEvents(); // discard events during replay
            }
            return clone;
        }

        public void RecordSyntheticMove(Move move)
        {
            moveHistory.Add(move);
        }
    }
}
