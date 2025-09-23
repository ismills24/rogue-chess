namespace RogueChess.Engine.Events
{
    public interface IInterceptor<TEvent>
        where TEvent : GameEvent
    {
        int Priority { get; }
        IEventSequence Intercept(TEvent ev, GameState state);
    }
}
