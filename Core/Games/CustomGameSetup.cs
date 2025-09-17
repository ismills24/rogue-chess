using ChessRogue.Core;
using ChessRogue.Core.Board;
using ChessRogue.Core.Board.Tiles;
using ChessRogue.Core.Pieces.Decorators;
using ChessRogue.Core.RuleSets;

public static class CustomGameSetup
{
    public static (GameState state, IRuleSet ruleset) Create()
    {
        var board = new StandardBoard(6, 6);

        // Add some slippery tiles
        board.SetTile(new Vector2Int(2, 2), new SlipperyTile());
        board.SetTile(new Vector2Int(3, 3), new SlipperyTile());

        // White: King + Pawn
        board.PlacePiece(new King(PlayerColor.White, new Vector2Int(0, 0)), new Vector2Int(0, 0));
        board.PlacePiece(new Pawn(PlayerColor.White, new Vector2Int(1, 1)), new Vector2Int(1, 1));

        // Black: Exploding Rook
        var explodingRook = new ExplodingPieceDecorator(
            new Rook(PlayerColor.Black, new Vector2Int(5, 5))
        );
        board.PlacePiece(explodingRook, new Vector2Int(5, 5));
        board.PlacePiece(new King(PlayerColor.Black, new Vector2Int(0, 5)), new Vector2Int(0, 5));

        var state = new GameState(board, PlayerColor.White);
        return (state, new LastPieceStandingRuleSet());
    }
}
