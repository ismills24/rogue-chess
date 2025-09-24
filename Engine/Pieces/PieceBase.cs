using System;
using System.Collections.Generic;
using System.Linq;
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
        public Guid ID { get; }
        public string Name { get; }
        public PlayerColor Owner { get; }
        public Vector2Int Position { get; set; }
        public int MovesMade { get; set; }
        public int CapturesMade { get; set; }

        protected PieceBase(string name, PlayerColor owner, Vector2Int position)
        {
            ID = Guid.NewGuid();
            Name = name;
            Owner = owner;
            Position = position;
            MovesMade = 0;
            CapturesMade = 0;
        }

        protected PieceBase(PieceBase original)
        {
            ID = original.ID;
            Name = original.Name;
            Owner = original.Owner;
            Position = original.Position;
            MovesMade = original.MovesMade;
            CapturesMade = original.CapturesMade;
        }

        // ---------------- Movement ----------------
        public abstract IEnumerable<Move> GetPseudoLegalMoves(GameState state);

        // ---------------- Value & Cloning ----------------
        public abstract int GetValue();

        public virtual IPiece Clone()
        {
            return (IPiece)MemberwiseClone();
        }

        public override string ToString() => $"{Name} ({Owner}) at {Position}";

        public void IncrementMoves()
        {
            MovesMade++;
        }

        // Call when the piece captures
        public void IncrementCaptures()
        {
            CapturesMade++;
        }
    }
}
