using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.Primitives
{
    /// <summary>
    /// Represents a single attempted move of a piece.
    /// Note: this is a proposition until confirmed by the Runner.
    /// </summary>
    public class Move
    {
        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public IPiece Piece { get; }
        public bool IsCapture { get; }
        public bool IsPromotion { get; }

        public Move(Vector2Int from, Vector2Int to, IPiece piece, bool isCapture = false, bool isPromotion = false)
        {
            From = from;
            To = to;
            Piece = piece;
            IsCapture = isCapture;
            IsPromotion = isPromotion;
        }
    }
}


