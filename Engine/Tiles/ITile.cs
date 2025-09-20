using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    public interface ITile
    {
        Vector2Int Position { get; set; }

        IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state);
        IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state);

        ITile Clone();
    }
}
