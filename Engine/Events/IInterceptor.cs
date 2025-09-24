using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.Events
{
    public interface IInterceptor<TEvent>
        where TEvent : GameEvent
    {
        int Priority { get; }
        IEventSequence Intercept(TEvent ev, GameState state);
    }

    public static class InterceptorGuards
    {
        /// <summary>
        /// True if this piece is the target of the event (Destroy/Capture).
        /// </summary>
        public static bool IsTarget(this IPiece self, GameEvent ev)
        {
            return ev switch
            {
                DestroyEvent de => ReferenceEquals(de.Target, self),
                CaptureEvent ce => ReferenceEquals(ce.Target, self),
                _ => false,
            };
        }

        /// <summary>
        /// True if this piece is the source/emitter of the event.
        /// Requires events to carry SourceId.
        /// </summary>
        public static bool IsSource(this IPiece self, GameEvent ev)
        {
            return ev.SourceID == self.ID;
        }

        /// <summary>
        /// True if this piece is the attacker/actor in a capture event.
        /// </summary>
        public static bool IsAttacker(this IPiece self, GameEvent ev)
        {
            return ev is CaptureEvent ce && ReferenceEquals(ce.Attacker, self);
        }
    }
}
