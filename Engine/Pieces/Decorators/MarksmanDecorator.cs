using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Allows the piece to capture enemy pieces from a distance without moving.
    /// When capturing, the piece stays on its current square instead of moving to the target.
    /// </summary>
    public class MarksmanDecorator : PieceDecoratorBase
    {
        private int _rangedAttacksLeft = 1;

        public MarksmanDecorator(IPiece inner)
            : base(inner) { }

        private MarksmanDecorator(IPiece inner, int rangedAttacksLeft)
            : base(inner)
        {
            _rangedAttacksLeft = rangedAttacksLeft;
        }

        /// <summary>
        /// Override move generation to include ranged capture moves.
        /// For each normal move that would land on an enemy piece, also create a ranged capture version.
        /// Only generates ranged captures if charges are available.
        /// </summary>
        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            // First, get all normal moves from the inner piece (including any decorators)
            foreach (var move in Inner.GetPseudoLegalMoves(state))
            {
                yield return move;
                
                // Only create ranged capture moves if we have charges left
                if (_rangedAttacksLeft > 0)
                {
                    // If this move would land on an enemy piece, also create a ranged capture version
                    var target = state.Board.GetPieceAt(move.To);
                    if (target != null && target.Owner != Inner.Owner)
                    {
                        // Create a ranged capture move where the piece doesn't move (From == To)
                        // Store the target position in the To field for reference
                        yield return new Move(move.From, move.To, Inner, true);
                    }
                }
            }
        }


        /// <summary>
        /// When a ranged capture is made, the piece moves to the target but then moves back.
        /// We generate a "move back" effect to return the piece to its original position.
        /// After using the ranged attack, one charge is consumed.
        /// </summary>
        public override IEnumerable<CandidateEvent> OnMove(Move move, GameState state)
        {
            // Check if this is a ranged capture (From != To but piece doesn't actually move)
            if (move.From != move.To && move.IsCapture && _rangedAttacksLeft > 0)
            {
                // This is a ranged capture - consume one charge
                _rangedAttacksLeft--;
                
                // First let the normal move processing happen
                foreach (var ev in Inner.OnMove(move, state))
                {
                    yield return ev;
                }
                
                // Then generate a "move back" effect to return the piece to its original position
                yield return new CandidateEvent(
                    GameEventType.MoveApplied,
                    false, // Not a player action
                    new MovePayload(Inner, move.To, move.From) // Move back to original position
                );
            }
            else
            {
                // Normal move - delegate to inner piece
                foreach (var ev in Inner.OnMove(move, state))
                {
                    yield return ev;
                }
            }
        }

        /// <summary>
        /// Increase the piece's value since it can capture from range.
        /// </summary>
        public override int GetValue()
        {
            return Inner.GetValue() + 2; // +2 for ranged capture ability
        }

        // Remove the BeforeEvent method - we'll handle this differently

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new MarksmanDecorator(inner, _rangedAttacksLeft);
    }
}
