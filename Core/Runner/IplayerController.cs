namespace ChessRogue.Core.Runner
{
    public interface IPlayerController
    {
        Move SelectMove(GameState state);
    }
}
