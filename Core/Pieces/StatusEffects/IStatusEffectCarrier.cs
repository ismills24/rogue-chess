namespace ChessRogue.Core.StatusEffects
{
    public interface IStatusEffectCarrier
    {
        void AddStatus(IStatusEffect effect);
        void RemoveStatus(string name);
        IReadOnlyList<IStatusEffect> GetStatuses();
    }
}
