using System;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;
using RogueChess.Engine.Tiles;

namespace RogueChess.Engine.Events
{
    /// <summary>
    /// Base class for all events in the system.
    /// Immutable, with a clear source and description.
    /// </summary>
    public abstract class GameEvent
    {
        public Guid Id { get; }
        public PlayerColor Actor { get; }
        public bool IsPlayerAction { get; }
        public string Description { get; }

        protected GameEvent(PlayerColor actor, bool isPlayerAction, string description)
        {
            Id = Guid.NewGuid();
            Actor = actor;
            IsPlayerAction = isPlayerAction;
            Description = description ?? string.Empty;
        }
    }

    public sealed class MoveEvent : GameEvent
    {
        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public IPiece Piece { get; }

        public MoveEvent(
            Vector2Int from,
            Vector2Int to,
            IPiece piece,
            PlayerColor actor,
            bool isPlayerAction
        )
            : base(actor, isPlayerAction, $"{piece.Name} moves {from} → {to}")
        {
            From = from;
            To = to;
            Piece = piece;
        }
    }

    public sealed class CaptureEvent : GameEvent
    {
        public IPiece Attacker { get; }
        public IPiece Target { get; }

        public CaptureEvent(IPiece attacker, IPiece target, PlayerColor actor, bool isPlayerAction)
            : base(actor, isPlayerAction, $"{attacker.Name} captures {target.Name}")
        {
            Attacker = attacker;
            Target = target;
        }
    }

    public sealed class DestroyEvent : GameEvent
    {
        public IPiece Target { get; }

        public DestroyEvent(IPiece target, string reason, PlayerColor actor)
            : base(actor, false, $"Destroy {target.Name}: {reason}")
        {
            Target = target;
        }
    }

    public sealed class StatusAppliedEvent : GameEvent
    {
        public IPiece Target { get; }
        public IStatusEffect Effect { get; }

        public StatusAppliedEvent(IPiece target, IStatusEffect effect, PlayerColor actor)
            : base(actor, false, $"Applied {effect.Name} to {target.Name}")
        {
            Target = target;
            Effect = effect;
        }
    }

    public sealed class StatusRemovedEvent : GameEvent
    {
        public IPiece Target { get; }
        public IStatusEffect Effect { get; }

        public StatusRemovedEvent(IPiece target, IStatusEffect effect, PlayerColor actor)
            : base(actor, false, $"Removed {effect.Name} from {target.Name}")
        {
            Target = target;
            Effect = effect;
        }
    }

    public sealed class TurnAdvancedEvent : GameEvent
    {
        public PlayerColor NextPlayer { get; }
        public int TurnNumber { get; }

        public TurnAdvancedEvent(PlayerColor nextPlayer, int turnNumber)
            : base(nextPlayer, false, $"Turn {turnNumber} → {nextPlayer}")
        {
            NextPlayer = nextPlayer;
            TurnNumber = turnNumber;
        }
    }

    public sealed class TurnStartEvent : GameEvent
    {
        public PlayerColor Player { get; }
        public int TurnNumber { get; }

        public TurnStartEvent(PlayerColor player, int turnNumber)
            : base(player, false, $"Turn {turnNumber} start for {player}")
        {
            Player = player;
            TurnNumber = turnNumber;
        }
    }

    public sealed class TurnEndEvent : GameEvent
    {
        public PlayerColor Player { get; }
        public int TurnNumber { get; }

        public TurnEndEvent(PlayerColor player, int turnNumber)
            : base(player, false, $"Turn {turnNumber} end for {player}")
        {
            Player = player;
            TurnNumber = turnNumber;
        }
    }

    public sealed class StatusTickEvent : GameEvent
    {
        public IPiece Target { get; }
        public IStatusEffect Effect { get; }
        public int RemainingDuration { get; }

        public StatusTickEvent(
            IPiece target,
            IStatusEffect effect,
            int remaining,
            PlayerColor actor
        )
            : base(actor, false, $"{effect.Name} ticks on {target.Name} (remaining {remaining})")
        {
            Target = target;
            Effect = effect;
            RemainingDuration = remaining;
        }
    }

    public sealed class TileChangedEvent : GameEvent
    {
        public Vector2Int Position { get; }
        public ITile NewTile { get; }

        public TileChangedEvent(Vector2Int position, ITile newTile, PlayerColor actor)
            : base(actor, false, $"Tile at {position} changed to {newTile.GetType().Name}")
        {
            Position = position;
            NewTile = newTile;
        }
    }

    // in GameEvent.cs
    public sealed class CommittedMoveEvent : GameEvent
    {
        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public IPiece Piece { get; }

        public CommittedMoveEvent(
            Vector2Int from,
            Vector2Int to,
            IPiece piece,
            PlayerColor actor,
            bool isPlayerAction = false
        )
            : base(actor, isPlayerAction, $"{piece.Name} {from}→{to} (committed)")
        {
            From = from;
            To = to;
            Piece = piece;
        }
    }
}
