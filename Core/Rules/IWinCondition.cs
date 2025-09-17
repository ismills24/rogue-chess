namespace ChessRogue.Core.Rules
{
    public interface IWinCondition
    {
        bool IsGameOver(GameState state, out PlayerColor winner);
    }
}
