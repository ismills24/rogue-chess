namespace ChessRogue.Core.Pieces.Decorators
{
    /// <summary>
    /// Base class for piece decorators — forwards everything to the inner piece
    /// unless overridden.
    /// </summary>
    public abstract class PieceDecoratorBase : IPiece
    {
        protected readonly IPiece inner;

        protected PieceDecoratorBase(IPiece inner)
        {
            this.inner = inner;
        }

        public virtual PlayerColor Owner => inner.Owner;

        public virtual Vector2Int Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public virtual string Name => inner.Name;

        public virtual IEnumerable<Move> GetPseudoLegalMoves(GameState state) =>
            inner.GetPseudoLegalMoves(state);

        public virtual void OnMove(Move move, GameState state) => inner.OnMove(move, state);

        public virtual void OnCapture(GameState state) => inner.OnCapture(state);

        public abstract IPiece Clone();
    }
}
