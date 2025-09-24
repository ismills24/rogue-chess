using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueChess.Engine.Events
{
    public static class EventSequences
    {
        /// <summary>
        /// No-op: event proceeds canonically.
        /// </summary>
        public static readonly IEventSequence Continue = new EventSequence(
            Array.Empty<GameEvent>(),
            FallbackPolicy.ContinueChain
        );

        /// <summary>
        /// Suppress the event entirely.
        /// </summary>
        public static readonly IEventSequence Abort = new EventSequence(
            Array.Empty<GameEvent>(),
            FallbackPolicy.AbortChain
        );

        /// <summary>
        /// Wrap one event with Continue fallback.
        /// </summary>
        public static IEventSequence Single(GameEvent ev) =>
            new EventSequence(new[] { ev }, FallbackPolicy.ContinueChain);

        /// <summary>
        /// Wrap many events with Continue fallback.
        /// </summary>
        public static IEventSequence Many(IEnumerable<GameEvent> evs) =>
            new EventSequence(evs.ToList(), FallbackPolicy.ContinueChain);
    }
}
