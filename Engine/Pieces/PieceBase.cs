using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Base class for all pieces providing default behavior.
    /// Concrete pieces should inherit from this and override specific methods.
    /// </summary>
    public abstract class PieceBase : IPiece
    {
        public string Name { get; }
        public PlayerColor Owner { get; }
        public Vector2Int Position { get; set; }

        protected PieceBase(string name, PlayerColor owner, Vector2Int position)
        {
            Name = name;
            Owner = owner;
            Position = position;
        }

        /// <summary>
        /// Override this to provide piece-specific move generation.
        /// </summary>
        public abstract IEnumerable<Move> GetPseudoLegalMoves(GameState state);

        /// <summary>
        /// Default behavior: no events on move.
        /// Override to add piece-specific move effects.
        /// </summary>
        public virtual IEnumerable<CandidateEvent> OnMove(Move move, GameState state)
        {
            yield break; // No effects by default
        }

        /// <summary>
        /// Default behavior: no events on capture.
        /// Override to add piece-specific capture effects.
        /// </summary>
        public virtual IEnumerable<CandidateEvent> OnCapture(GameState state)
        {
            yield break; // No effects by default
        }

        /// <summary>
        /// Override this to provide piece-specific value.
        /// </summary>
        public abstract int GetValue();

        /// <summary>
        /// Default cloning behavior.
        /// Override if piece has additional state to clone.
        /// </summary>
        public virtual IPiece Clone()
        {
            // Use reflection to create a new instance of the same type
            var constructor = GetType().GetConstructor(new[] { typeof(string), typeof(PlayerColor), typeof(Vector2Int) });
            if (constructor != null)
            {
                return (IPiece)constructor.Invoke(new object[] { Name, Owner, Position });
            }
            
            // Fallback: create a copy with the same properties
            return CreateClone();
        }

        /// <summary>
        /// Override this method to provide custom cloning logic.
        /// </summary>
        protected virtual IPiece CreateClone()
        {
            throw new NotImplementedException($"Clone not implemented for {GetType().Name}");
        }

        public override string ToString()
        {
            return $"{Name} ({Owner}) at {Position}";
        }
    }
}
