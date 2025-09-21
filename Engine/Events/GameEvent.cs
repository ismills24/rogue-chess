using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;
using RogueChess.Engine.Tiles;

namespace RogueChess.Engine.Events
{
    public enum GameEventType
    {
        MoveApplied,
        PieceCaptured,
        PiecePromoted,
        TileEffectTriggered,
        StatusEffectTriggered,
        TurnAdvanced,
        GameOver,
        PieceDestroyed,
        MoveCancelled,
        StatusTick,
    }

    /// <summary>
    /// Canonical event — published externally and stored in history.
    /// One canonical GameEvent <-> one GameState snapshot.
    /// </summary>
    public class GameEvent
    {
        public Guid Id { get; }
        public GameEventType Type { get; }
        public bool IsPlayerAction { get; }
        public Guid? ParentEventId { get; }
        public object? Payload { get; }

        public GameEvent(
            Guid id,
            GameEventType type,
            bool isPlayerAction,
            Guid? parentEventId,
            object? payload
        )
        {
            Id = id;
            Type = type;
            IsPlayerAction = isPlayerAction;
            ParentEventId = parentEventId;
            Payload = payload;
        }
    }

    /// <summary>
    /// Internal candidate event — proposed by pieces, tiles, statuses, etc.
    /// Will be validated/rewritten by hooks before becoming canonical.
    /// </summary>
    public class CandidateEvent
    {
        public GameEventType Type { get; }
        public bool IsPlayerAction { get; }
        public object? Payload { get; }

        public CandidateEvent(GameEventType type, bool isPlayerAction, object? payload)
        {
            Type = type;
            IsPlayerAction = isPlayerAction;
            Payload = payload;
        }
    }

    // ----------------------------
    // Payload types
    // ----------------------------

    public class MovePayload
    {
        public IPiece Piece { get; }
        public Vector2Int From { get; }
        public Vector2Int To { get; }

        public MovePayload(IPiece piece, Vector2Int from, Vector2Int to)
        {
            Piece = piece;
            From = from;
            To = to;
        }
    }

    public class CapturePayload
    {
        public IPiece Target { get; }

        public CapturePayload(IPiece target)
        {
            Target = target;
        }
    }

    public class TileChangePayload
    {
        public Vector2Int Position { get; }
        public ITile NewTile { get; }

        public TileChangePayload(Vector2Int position, ITile newTile)
        {
            Position = position;
            NewTile = newTile;
        }
    }

    public class StatusApplyPayload
    {
        public IPiece Target { get; }
        public IStatusEffect Effect { get; }

        public StatusApplyPayload(IPiece target, IStatusEffect effect)
        {
            Target = target;
            Effect = effect;
        }
    }

    public class ForcedSlidePayload
    {
        public IPiece Piece { get; }
        public Vector2Int From { get; }
        public Vector2Int To { get; }

        public ForcedSlidePayload(IPiece piece, Vector2Int from, Vector2Int to)
        {
            Piece = piece;
            From = from;
            To = to;
        }
    }
}
