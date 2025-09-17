namespace ChessRogue.Core.WinConditions
{
    public interface IWinCondition
    {
        bool IsGameOver(GameState state, out PlayerColor winner);
    }
}
