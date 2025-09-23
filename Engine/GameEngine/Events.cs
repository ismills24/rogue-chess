// Engine/GameEngine/Events.cs
using System.Collections.Generic;
using RogueChess.Engine.Events;

namespace RogueChess.Engine
{
    /// <summary>
    /// Thin helpers for constructing action packages (IEventSequence) used by the engine.
    /// Keep "action package" concerns separate from event/interceptor types.
    /// </summary>
    public static class ActionPackages
    {
        public static EventSequence Pack(
            IEnumerable<GameEvent> events,
            FallbackPolicy fallback = FallbackPolicy.AbortChain
        ) => new EventSequence(events, fallback);

        public static EventSequence Single(
            GameEvent ev,
            FallbackPolicy fallback = FallbackPolicy.AbortChain
        ) => new EventSequence(new[] { ev }, fallback);

        public static readonly EventSequence EmptyContinue = new EventSequence(
            new GameEvent[0],
            FallbackPolicy.ContinueChain
        );
        public static readonly EventSequence EmptyAbort = new EventSequence(
            new GameEvent[0],
            FallbackPolicy.AbortChain
        );
    }
}
