using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Base class for piece decorators that wrap other pieces.
    /// Forwards all calls to the inner piece unless overridden.
    /// </summary>
    public abstract class PieceDecoratorBase : IPiece
    {
        public IPiece Inner { get; }

        protected PieceDecoratorBase(IPiece inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public virtual string Name => Inner.Name;
        public virtual PlayerColor Owner => Inner.Owner;
        public virtual Vector2Int Position
        {
            get => Inner.Position;
            set => Inner.Position = value;
        }
        public virtual int MovesMade
        {
            get => Inner.MovesMade;
            set => Inner.MovesMade = value;
        }
        public virtual int CapturesMade
        {
            get => Inner.CapturesMade;
            set => Inner.CapturesMade = value;
        }

        /// <summary>
        /// Default behavior: forward to inner piece.
        /// Override to modify or add to the move generation.
        /// </summary>
        public virtual IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            return Inner.GetPseudoLegalMoves(state);
        }

        /// <summary>
        /// Default behavior: forward to inner piece, then add decorator effects.
        /// Override to modify the behavior.
        /// </summary>
        public virtual IEnumerable<CandidateEvent> OnMove(Move move, GameState state)
        {
            // First, let the inner piece handle the move
            foreach (var ev in Inner.OnMove(move, state))
            {
                yield return ev;
            }

            // Then add any decorator-specific effects
            foreach (var ev in OnMoveDecorator(move, state))
            {
                yield return ev;
            }
        }

        /// <summary>
        /// Default behavior: forward to inner piece, then add decorator effects.
        /// Override to modify the behavior.
        /// </summary>
        public virtual IEnumerable<CandidateEvent> OnCapture(GameState state)
        {
            // First, let the inner piece handle the capture
            foreach (var ev in Inner.OnCapture(state))
            {
                yield return ev;
            }

            // Then add any decorator-specific effects
            foreach (var ev in OnCaptureDecorator(state))
            {
                yield return ev;
            }
        }

        /// <summary>
        /// Default behavior: forward to inner piece.
        /// Override to modify the piece value.
        /// </summary>
        public virtual int GetValue()
        {
            return Inner.GetValue();
        }

        /// <summary>
        /// Default behavior: clone the decorator with a cloned inner piece.
        /// Override to provide custom cloning logic.
        /// </summary>
        public virtual IPiece Clone()
        {
            var clonedInner = Inner.Clone();
            return CreateDecoratorClone(clonedInner);
        }

        /// <summary>
        /// Override this to provide decorator-specific move effects.
        /// </summary>
        protected virtual IEnumerable<CandidateEvent> OnMoveDecorator(Move move, GameState state)
        {
            yield break; // No effects by default
        }

        /// <summary>
        /// Override this to provide decorator-specific capture effects.
        /// </summary>
        protected virtual IEnumerable<CandidateEvent> OnCaptureDecorator(GameState state)
        {
            yield break; // No effects by default
        }

        /// <summary>
        /// Override this to create a new instance of the decorator with the given inner piece.
        /// </summary>
        protected abstract IPiece CreateDecoratorClone(IPiece inner);

        public override string ToString()
        {
            return $"{GetType().Name}({Inner})";
        }

        public virtual IEnumerable<CandidateEvent> OnTurnStart(GameState state)
        {
            return Inner.OnTurnStart(state);
        }

        public virtual IEnumerable<CandidateEvent> OnTurnEnd(GameState state)
        {
            return Inner.OnTurnEnd(state);
        }
    }
}
