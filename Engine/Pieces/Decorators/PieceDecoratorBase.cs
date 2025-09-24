using System;
using System.Collections.Generic;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    public abstract class PieceDecoratorBase : IPiece
    {
        public IPiece Inner { get; }
        public Guid ID { get; }

        protected PieceDecoratorBase(IPiece inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            ID = Guid.NewGuid();
        }

        protected PieceDecoratorBase(PieceDecoratorBase original, IPiece innerClone)
        {
            ID = original.ID; // preserve same ID
            Inner = innerClone; // clone of wrapped piece
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

        public virtual IEnumerable<Move> GetPseudoLegalMoves(GameState state) =>
            Inner.GetPseudoLegalMoves(state);

        public virtual int GetValue() => Inner.GetValue();

        public virtual IPiece Clone()
        {
            var innerClone = Inner.Clone();
            var ctor = GetType().GetConstructor(new[] { GetType(), typeof(IPiece) });
            if (ctor != null)
                return (IPiece)ctor.Invoke(new object[] { this, innerClone });

            return CreateDecoratorClone(innerClone); // fallback to manual
        }

        protected abstract IPiece CreateDecoratorClone(IPiece inner);

        public override string ToString() => $"{GetType().Name}({Inner})";
    }
}
