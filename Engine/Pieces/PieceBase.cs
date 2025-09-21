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
        public string Name { get; }
        public PlayerColor Owner { get; }
        public Vector2Int Position { get; set; }
        public int MovesMade { get; set; }
        public int CapturesMade { get; set; }

        protected PieceBase(string name, PlayerColor owner, Vector2Int position)
        {
            Name = name;
            Owner = owner;
            Position = position;
            MovesMade = 0;
            CapturesMade = 0;
        }

        // ---------------- Movement ----------------
        public abstract IEnumerable<Move> GetPseudoLegalMoves(GameState state);

        // ---------------- Event hooks ----------------
        public virtual IEnumerable<CandidateEvent> OnMove(Move move, GameState state)
        {
            yield break;
        }

        public virtual IEnumerable<CandidateEvent> OnCapture(GameState state)
        {
            yield break;
        }

        public virtual IEnumerable<CandidateEvent> OnTurnStart(GameState state)
        {
            yield break;
        }

        public virtual IEnumerable<CandidateEvent> OnTurnEnd(GameState state)
        {
            yield break;
        }

        // ---------------- Value & Cloning ----------------
        public abstract int GetValue();

        public IPiece Clone() => CreateClone();

        /// <summary>
        /// Concrete pieces must implement cloning.
        /// </summary>
        protected abstract IPiece CreateClone();

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



