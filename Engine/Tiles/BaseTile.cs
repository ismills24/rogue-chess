using System;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    public abstract class BaseTile : ITile
    {
        public Vector2Int Position { get; set; }
        public Guid ID { get; }

        protected BaseTile()
        {
            ID = Guid.NewGuid();
            Position = new Vector2Int(0, 0);
        }

        protected BaseTile(Vector2Int position)
        {
            ID = Guid.NewGuid();
            Position = position;
        }

        protected BaseTile(BaseTile original)
        {
            ID = original.ID; // preserve same identity
            Position = original.Position;
        }

        public virtual ITile Clone()
        {
            // Use reflection: assume tile has a copy ctor (BaseTile original)
            var ctor = GetType().GetConstructor(new[] { GetType() });
            if (ctor != null)
                return (ITile)ctor.Invoke(new object[] { this });

            throw new InvalidOperationException($"No copy constructor for {GetType().Name}");
        }
    }
}
