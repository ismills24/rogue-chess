using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine.Pieces.Decorators
{
    public class StatusEffectDecorator
        : PieceDecoratorBase,
            IInterceptor<TurnStartEvent>,
            IInterceptor<TurnEndEvent>
    {
        private readonly List<IStatusEffect> _statuses;

        public StatusEffectDecorator(IPiece inner)
            : base(inner)
        {
            _statuses = new List<IStatusEffect>();
        }

        public StatusEffectDecorator(PieceDecoratorBase original, IPiece innerClone)
            : base(original, innerClone) { }

        private StatusEffectDecorator(IPiece inner, List<IStatusEffect> statuses)
            : base(inner)
        {
            _statuses = statuses;
        }

        public void AddStatus(IStatusEffect status) => _statuses.Add(status);

        public bool HasAnyStatus => _statuses.Count > 0;

        public bool RemoveStatus(IStatusEffect status)
        {
            // Prefer reference equality if the same instance was carried
            var idx = _statuses.FindIndex(s => ReferenceEquals(s, status));
            if (idx >= 0)
            {
                _statuses.RemoveAt(idx);
                return true;
            }

            // Fallback: remove by name (handles cloned/serialized effects)
            idx = _statuses.FindIndex(s =>
                string.Equals(s.Name, status.Name, StringComparison.Ordinal)
            );
            if (idx >= 0)
            {
                _statuses.RemoveAt(idx);
                return true;
            }

            return false;
        }

        public IEnumerable<IStatusEffect> GetStatuses() => _statuses.AsReadOnly();

        public int Priority => 0;

        public IEventSequence Intercept(TurnStartEvent ev, GameState state)
        {
            if (ev.Player != Inner.Owner)
                return new EventSequence(Array.Empty<GameEvent>(), FallbackPolicy.ContinueChain);

            var events = new List<GameEvent>();
            foreach (var status in _statuses.ToList())
                events.AddRange(status.OnTurnStart(Inner, state));

            return new EventSequence(events, FallbackPolicy.ContinueChain);
        }

        public IEventSequence Intercept(TurnEndEvent ev, GameState state)
        {
            if (ev.Player != Inner.Owner)
                return new EventSequence(Array.Empty<GameEvent>(), FallbackPolicy.ContinueChain);

            var events = new List<GameEvent>();
            foreach (var status in _statuses.ToList())
                events.AddRange(status.OnTurnEnd(Inner, state));

            return new EventSequence(events, FallbackPolicy.ContinueChain);
        }

        public override int GetValue()
        {
            var baseValue = Inner.GetValue();
            var penalty = _statuses.Sum(s => s.ValueModifier());
            return baseValue + penalty;
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new StatusEffectDecorator(inner);
    }
}
