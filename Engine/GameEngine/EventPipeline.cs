// Engine/GameEngine/GameEngine.EventPipeline.cs
using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Pieces.Decorators;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// Execute an EventSequence through the interceptor pipeline, mutating state
        /// with ApplyEventToState for any event that survives interception.
        /// Returns true if the whole sequence ran to completion, false if it was aborted
        /// by an interceptor that returned an empty sequence with AbortChain.
        /// </summary>
        public bool Dispatch(IEventSequence sequence, bool simulation = false)
        {
            Console.WriteLine(
                $"[Pipeline] Dispatch start: {sequence.Events.Count} events, sim={simulation}"
            );
            var stack = new Stack<GameEvent>(sequence.Events.Reverse());

            while (stack.Count > 0)
            {
                var ev = stack.Pop();
                Console.WriteLine($"[Pipeline] Handling {ev.GetType().Name} ({ev.Description})");

                var handled = TryInterceptOnce(ev, out var replacement, CurrentState);

                if (handled)
                {
                    Console.WriteLine(
                        $"[Pipeline] Intercepted {ev.GetType().Name} -> {replacement.Events.Count} events, Fallback={replacement.Fallback}"
                    );
                    if (replacement.Events.Count == 0)
                    {
                        if (replacement.Fallback == FallbackPolicy.AbortChain)
                        {
                            Console.WriteLine("[Pipeline] Aborting sequence");
                            return false;
                        }
                        Console.WriteLine("[Pipeline] Skipping event");
                        continue;
                    }

                    // push replacements
                    for (int i = replacement.Events.Count - 1; i >= 0; i--)
                        stack.Push(replacement.Events[i]);

                    continue; // don't fall through
                }

                Console.WriteLine($"[Pipeline] Applying canonical {ev.GetType().Name}");
                ApplyCanonical(ev, simulation);
            }

            Console.WriteLine("[Pipeline] Sequence completed");
            return true;
        }

        /// <summary>
        /// Attempts to find and invoke exactly one interceptor for the given event (highest priority first).
        /// If found, returns true and outputs a replacement sequence.
        /// If none matches, returns false and 'replacement' is null.
        /// </summary>
        private bool TryInterceptOnce(GameEvent ev, out IEventSequence replacement, GameState state)
        {
            replacement = null;
            var interceptors = InterceptorCollector.GetForEvent(ev, state);
            Console.WriteLine(
                $"[Pipeline] Found {interceptors.Count} interceptors for {ev.GetType().Name}"
            );

            if (interceptors.Count == 0)
                return false;

            var chosen = interceptors[0];
            Console.WriteLine(
                $"[Pipeline] Using interceptor {chosen.Instance.GetType().Name} (Priority={chosen.Priority})"
            );
            replacement = InterceptorCollector.InvokeIntercept(chosen.Instance, ev, state);
            return true;
        }
    }

    /// <summary>
    /// Gathers & invokes IInterceptor&lt;TEvent&gt; implementations using reflection.
    /// No changes to your interceptor classes are required.
    /// </summary>
    internal static class InterceptorCollector
    {
        internal struct Match
        {
            public object Instance;
            public int Priority;
            public Type TargetType;
        }

        // Collect every object on the board that implements IInterceptor<T>
        public static List<object> CollectAll(GameState state)
        {
            var results = new List<object>();

            // 1) Tiles
            foreach (var tile in state.Board.GetAllTiles())
            {
                if (ImplementsAnyInterceptor(tile))
                    results.Add(tile);
            }

            // 2) Pieces (walk decorator chains)
            foreach (var piece in state.Board.GetAllPieces())
            {
                var current = piece;
                while (true)
                {
                    if (ImplementsAnyInterceptor(current))
                        results.Add(current);

                    if (current is PieceDecoratorBase deco)
                    {
                        current = deco.Inner;
                        continue;
                    }
                    break;
                }
            }

            return results;
        }

        // Filter to interceptors that can handle this event, ordered by priority (low first),
        // then by specificity (exact T == ev.GetType() preferred over base classes).
        public static List<Match> GetForEvent(GameEvent ev, GameState state)
        {
            var all = CollectAll(state);
            var matches = new List<Match>();

            var evType = ev.GetType();

            foreach (var obj in all)
            {
                foreach (var iface in obj.GetType().GetInterfaces())
                {
                    if (!iface.IsGenericType)
                        continue;

                    if (iface.GetGenericTypeDefinition() != typeof(IInterceptor<>))
                        continue; // not an interceptor interface

                    var targetType = iface.GetGenericArguments()[0];

                    // we allow IInterceptor<Base> to match Derived events
                    if (!targetType.IsAssignableFrom(evType))
                        continue;

                    var priorityProp = iface.GetProperty("Priority");
                    var priority = (int)(priorityProp?.GetValue(obj) ?? 0);

                    matches.Add(
                        new Match
                        {
                            Instance = obj,
                            Priority = priority,
                            TargetType = targetType,
                        }
                    );
                }
            }

            // Order: priority asc, then "more specific" first (exact match before base)
            matches.Sort(
                (a, b) =>
                {
                    var byPriority = a.Priority.CompareTo(b.Priority);
                    if (byPriority != 0)
                        return byPriority;

                    // Specificity: if one target type equals the actual event type, prefer it
                    var aExact = a.TargetType == evType ? 0 : 1;
                    var bExact = b.TargetType == evType ? 0 : 1;
                    return aExact.CompareTo(bExact);
                }
            );

            return matches;
        }

        // Invoke IInterceptor<T>.Intercept(T ev, GameState) via reflection
        public static IEventSequence InvokeIntercept(
            object interceptor,
            GameEvent ev,
            GameState state
        )
        {
            // Find the matching IInterceptor<T> interface for this ev
            var evType = ev.GetType();
            var iface = interceptor
                .GetType()
                .GetInterfaces()
                .First(i =>
                    i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IInterceptor<>)
                    && i.GetGenericArguments()[0].IsAssignableFrom(evType)
                );

            var method = iface.GetMethod("Intercept");
            return (IEventSequence)method.Invoke(interceptor, new object[] { ev, state });
        }

        private static bool ImplementsAnyInterceptor(object obj)
        {
            return obj.GetType()
                .GetInterfaces()
                .Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IInterceptor<>)
                );
        }
    }
}
