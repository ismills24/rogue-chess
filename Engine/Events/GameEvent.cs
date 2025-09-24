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
        public Guid SourceID { get; }
        public PlayerColor Actor { get; }
        public bool IsPlayerAction { get; }
        public string Description { get; }

        protected GameEvent(
            PlayerColor actor,
            bool isPlayerAction,
            string description,
            Guid sourceId
        )
        {
            Id = Guid.NewGuid();
            Actor = actor;
            IsPlayerAction = isPlayerAction;
            Description = description ?? string.Empty;
            SourceID = sourceId;
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
            : base(actor, isPlayerAction, $"{piece.Name} moves {from} → {to}", piece.ID)
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
            : base(actor, isPlayerAction, $"{attacker.Name} captures {target.Name}", attacker.ID)
        {
            Attacker = attacker;
            Target = target;
        }
    }

    public sealed class DestroyEvent : GameEvent
    {
        public IPiece Target { get; }

        public DestroyEvent(IPiece target, string reason, PlayerColor actor, Guid sourceId)
            : base(actor, false, $"Destroy {target.Name}: {reason}", sourceId)
        {
            Target = target;
        }
    }

    public sealed class StatusAppliedEvent : GameEvent
    {
        public IPiece Target { get; }
        public IStatusEffect Effect { get; }

        public StatusAppliedEvent(IPiece target, IStatusEffect effect, PlayerColor actor)
            : base(actor, false, $"Applied {effect.Name} to {target.Name}", effect.ID)
        {
            Target = target;
            Effect = effect;
        }
    }

    public sealed class StatusRemovedEvent : GameEvent
    {
        public IPiece Target { get; }
        public IStatusEffect Effect { get; }

        public StatusRemovedEvent(
            IPiece target,
            IStatusEffect effect,
            PlayerColor actor,
            Guid sourceId
        )
            : base(actor, false, $"Removed {effect.Name} from {target.Name}", sourceId)
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
            : base(nextPlayer, false, $"Turn {turnNumber} → {nextPlayer}", Guid.Empty)
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
            : base(player, false, $"Turn {turnNumber} start for {player}", Guid.Empty)
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
            : base(player, false, $"Turn {turnNumber} end for {player}", Guid.Empty)
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
            : base(
                actor,
                false,
                $"{effect.Name} ticks on {target.Name} (remaining {remaining})",
                effect.ID
            )
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
            : base(
                actor,
                false,
                $"Tile at {position} changed to {newTile.GetType().Name}",
                newTile.ID
            )
        {
            Position = position;
            NewTile = newTile;
        }
    }
}
