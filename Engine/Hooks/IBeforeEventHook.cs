using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Hooks
{
    /// <summary>
    /// Interface for global event interception hooks.
    /// Allows pieces, tiles, and other game objects to intercept and modify candidate events
    /// before they become canonical events.
    /// </summary>
    public interface IBeforeEventHook
    {
        /// <summary>
        /// Called before a candidate event is processed.
        /// Can return a modified candidate event or null to cancel the event.
        /// </summary>
        /// <param name="candidate">The candidate event being processed</param>
        /// <param name="state">Current game state</param>
        /// <returns>Modified candidate event, original candidate event, or null to cancel</returns>
        CandidateEvent? BeforeEvent(CandidateEvent candidate, GameState state);
    }
}
