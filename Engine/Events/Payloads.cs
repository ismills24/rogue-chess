using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Events
{
    /// <summary>Payload for when a piece is destroyed by an effect (explosion, burning, etc.).</summary>
    public class PieceDestroyedPayload
    {
        public IPiece Piece { get; }
        public string? Reason { get; }

        public PieceDestroyedPayload(IPiece piece, string? reason = null)
        {
            Piece = piece;
            Reason = reason;
        }
    }

    /// <summary>Payload for status effect ticking (duration updates).</summary>
    public class StatusTickPayload
    {
        public IPiece Piece { get; }
        public string StatusName { get; }
        public int RemainingDuration { get; }

        public StatusTickPayload(IPiece piece, string statusName, int remainingDuration)
        {
            Piece = piece;
            StatusName = statusName;
            RemainingDuration = remainingDuration;
        }
    }

    /// <summary>Payload for when a move is cancelled.</summary>
    public class MoveCancelledPayload
    {
        public IPiece Attacker { get; }
        public Move OriginalMove { get; }

        public MoveCancelledPayload(IPiece attacker, Move originalMove)
        {
            Attacker = attacker;
            OriginalMove = originalMove;
        }
    }

    /// <summary>Payload for when a piece is promoted.</summary>
    public class PiecePromotedPayload
    {
        public IPiece OldPiece { get; }
        public IPiece NewPiece { get; }
        public Vector2Int Position { get; }

        public PiecePromotedPayload(IPiece oldPiece, IPiece newPiece, Vector2Int position)
        {
            OldPiece = oldPiece;
            NewPiece = newPiece;
            Position = position;
        }
    }
}
