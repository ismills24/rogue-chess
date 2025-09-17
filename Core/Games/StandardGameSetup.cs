using ChessRogue.Core;
using ChessRogue.Core.Board;
using ChessRogue.Core.RuleSets;

public static class StandardGameSetup
{
    public static (GameState state, IRuleSet ruleset) Create()
    {
        var board = new StandardBoard(8, 8);

        // Pawns
        for (int x = 0; x < 8; x++)
        {
            board.PlacePiece(
                new Pawn(PlayerColor.White, new Vector2Int(x, 1)),
                new Vector2Int(x, 1)
            );
            board.PlacePiece(
                new Pawn(PlayerColor.Black, new Vector2Int(x, 6)),
                new Vector2Int(x, 6)
            );
        }

        // Rooks
        board.PlacePiece(new Rook(PlayerColor.White, new Vector2Int(0, 0)), new Vector2Int(0, 0));
        board.PlacePiece(new Rook(PlayerColor.White, new Vector2Int(7, 0)), new Vector2Int(7, 0));
        board.PlacePiece(new Rook(PlayerColor.Black, new Vector2Int(0, 7)), new Vector2Int(0, 7));
        board.PlacePiece(new Rook(PlayerColor.Black, new Vector2Int(7, 7)), new Vector2Int(7, 7));

        // Knights
        board.PlacePiece(new Knight(PlayerColor.White, new Vector2Int(1, 0)), new Vector2Int(1, 0));
        board.PlacePiece(new Knight(PlayerColor.White, new Vector2Int(6, 0)), new Vector2Int(6, 0));
        board.PlacePiece(new Knight(PlayerColor.Black, new Vector2Int(1, 7)), new Vector2Int(1, 7));
        board.PlacePiece(new Knight(PlayerColor.Black, new Vector2Int(6, 7)), new Vector2Int(6, 7));

        // Bishops
        board.PlacePiece(new Bishop(PlayerColor.White, new Vector2Int(2, 0)), new Vector2Int(2, 0));
        board.PlacePiece(new Bishop(PlayerColor.White, new Vector2Int(5, 0)), new Vector2Int(5, 0));
        board.PlacePiece(new Bishop(PlayerColor.Black, new Vector2Int(2, 7)), new Vector2Int(2, 7));
        board.PlacePiece(new Bishop(PlayerColor.Black, new Vector2Int(5, 7)), new Vector2Int(5, 7));

        // Queens
        board.PlacePiece(new Queen(PlayerColor.White, new Vector2Int(3, 0)), new Vector2Int(3, 0));
        board.PlacePiece(new Queen(PlayerColor.Black, new Vector2Int(3, 7)), new Vector2Int(3, 7));

        // Kings
        board.PlacePiece(new King(PlayerColor.White, new Vector2Int(4, 0)), new Vector2Int(4, 0));
        board.PlacePiece(new King(PlayerColor.Black, new Vector2Int(4, 7)), new Vector2Int(4, 7));

        var state = new GameState(board, PlayerColor.White);
        return (state, new StandardChessRuleSet());
    }
}
