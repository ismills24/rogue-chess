namespace ChessRogue.Core.Board
{
    public class StandardTile : ITile
    {
        public bool CanEnter(IPiece piece, Vector2Int pos, GameState state) => true;

        public void OnEnter(IPiece piece, Vector2Int pos, GameState state)
        {
            // normal tiles have no special effect
        }

        public void OnTurnStart(IPiece piece, Vector2Int pos, GameState state)
        {
            // nothing happens
        }
    }
}
