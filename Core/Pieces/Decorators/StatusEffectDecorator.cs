using ChessRogue.Core.Events;
using ChessRogue.Core.StatusEffects;

namespace ChessRogue.Core.Pieces.Decorators
{
    public class StatusEffectDecorator : PieceDecoratorBase, IStatusEffectCarrier
    {
        private readonly List<IStatusEffect> effects = new();

        public StatusEffectDecorator(IPiece inner)
            : base(inner) { }

        public override void OnCapture(GameState state)
        {
            base.OnCapture(state);
            foreach (var e in effects)
                e.OnRemove(inner, state);
        }

        public override IPiece Clone()
        {
            var clone = new StatusEffectDecorator(inner.Clone());
            foreach (var e in effects)
                clone.effects.Add(e.Clone());
            return clone;
        }

        // ---- IStatusEffectCarrier ----
        public void AddStatus(IStatusEffect effect) => effects.Add(effect);

        public void RemoveStatus(string name) => effects.RemoveAll(e => e.Name == name);

        public IReadOnlyList<IStatusEffect> GetStatuses() => effects.AsReadOnly();

        public IEnumerable<GameEvent> OnTurnStart(GameState state)
        {
            foreach (var e in effects.ToList()) // copy so removal is safe
            foreach (var ev in e.OnTurnStart(inner, state))
                yield return ev;
        }
    }
}
