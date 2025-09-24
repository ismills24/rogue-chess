using System.Collections.Generic;
using System.Linq;

namespace RogueChess.Engine.Events
{
    public enum FallbackPolicy
    {
        AbortChain,
        ContinueChain,
    }

    public interface IEventSequence
    {
        IReadOnlyList<GameEvent> Events { get; }
        FallbackPolicy Fallback { get; }
    }

    public class EventSequence : IEventSequence
    {
        public IReadOnlyList<GameEvent> Events { get; }
        public FallbackPolicy Fallback { get; }

        public EventSequence(
            IEnumerable<GameEvent> events,
            FallbackPolicy fallback = FallbackPolicy.AbortChain
        )
        {
            Events = events.ToList().AsReadOnly();
            Fallback = fallback;
        }
    }
}
