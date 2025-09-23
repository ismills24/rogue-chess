using System;
using System.Collections.Generic;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Base class for piece decorators that wrap other pieces.
    /// Forwards all calls to the inner piece unless overridden.
    ///
    /// Decorators no longer emit CandidateEvents directly — instead,
    /// they implement IInterceptor&lt;TEvent&gt; for the events they care about.
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
        /// Default behavior: forward to inner piece.
        /// Override to modify the piece value.
        /// </summary>
        public virtual int GetValue() => Inner.GetValue();

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
        /// Override this to create a new instance of the decorator with the given inner piece.
        /// </summary>
        protected abstract IPiece CreateDecoratorClone(IPiece inner);

        public override string ToString() => $"{GetType().Name}({Inner})";
    }
}
