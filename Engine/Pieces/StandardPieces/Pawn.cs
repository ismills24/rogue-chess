using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Standard chess pawn piece.
    /// Handles single/double forward moves and diagonal captures.
    /// </summary>
    public class Pawn : PieceBase, IInterceptor<MoveEvent>
    {
        public Pawn(PlayerColor owner, Vector2Int position)
            : base("Pawn", owner, position) { }

        public Pawn(Pawn original)
            : base(original) { }

        public override int GetValue() => 1;

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            var moves = new List<Move>();
            var direction = Owner == PlayerColor.White ? 1 : -1;
            var startRank = Owner == PlayerColor.White ? 1 : state.Board.Height - 2;

            // Forward 1
            var forwardOne = Position + new Vector2Int(0, direction);
            if (state.Board.IsInBounds(forwardOne) && state.Board.GetPieceAt(forwardOne) == null)
            {
                moves.Add(new Move(Position, forwardOne, this));
            }

            // Forward 2 (from start rank)
            if (Position.Y == startRank)
            {
                var forwardTwo = Position + new Vector2Int(0, direction * 2);
                if (
                    state.Board.IsInBounds(forwardTwo)
                    && state.Board.GetPieceAt(forwardTwo) == null
                    && state.Board.GetPieceAt(forwardOne) == null
                )
                {
                    moves.Add(new Move(Position, forwardTwo, this));
                }
            }

            // Diagonal captures
            foreach (var offset in new[] { -1, 1 })
            {
                var diag = Position + new Vector2Int(offset, direction);
                if (state.Board.IsInBounds(diag))
                {
                    var target = state.Board.GetPieceAt(diag);
                    if (target != null && target.Owner != Owner)
                    {
                        moves.Add(new Move(Position, diag, this, isCapture: true));
                    }
                }
            }

            return moves;
        }

        public int Priority => 0;

        public IEventSequence Intercept(MoveEvent ev, GameState state)
        {
            Console.WriteLine($"Intercepting MoveEvent: {ev.Piece.ID} {this.ID}");
            // Only intercept if this pawn is the mover and it's reached the last rank
            if (ev.Piece.ID != this.ID)
                return EventSequences.Continue;
            Console.WriteLine($"Intercepting MoveEvent: {ev.Piece.Name} moves {ev.From} → {ev.To}");
            int lastRank = Owner == PlayerColor.White ? state.Board.Height - 1 : 0;
            if (ev.To.Y != lastRank)
                return EventSequences.Continue;

            return EventSequences.Single(
                new PieceChangedEvent(
                    this,
                    new Queen(Owner, ev.To),
                    ev.To,
                    ev.Actor,
                    isPlayerAction: ev.IsPlayerAction,
                    sourceId: ID
                )
            );
        }
    }
}
