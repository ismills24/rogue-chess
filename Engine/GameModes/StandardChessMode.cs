using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.StatusEffects;
using RogueChess.Engine.Tiles;

namespace RogueChess.Engine.GameModes
{
    /// <summary>
    /// Standard chess game mode with traditional pieces and no special tiles
    /// </summary>
    public class StandardChessMode : IGameMode
    {
        public string Name => "Standard Chess";
        public string Description =>
            "Traditional chess with standard pieces and no special effects";

        public IBoard SetupBoard()
        {
            var board = new RogueChess.Engine.Board.Board(8, 8);

            // All tiles are standard (default)
            // Set up standard chess pieces

            // White pieces
            board.PlacePiece(
                new Rook(PlayerColor.White, new Vector2Int(0, 0)),
                new Vector2Int(0, 0)
            );
            board.PlacePiece(
                new Knight(PlayerColor.White, new Vector2Int(1, 0)),
                new Vector2Int(1, 0)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.White, new Vector2Int(2, 0)),
                new Vector2Int(2, 0)
            );
            board.PlacePiece(
                new Queen(PlayerColor.White, new Vector2Int(3, 0)),
                new Vector2Int(3, 0)
            );
            board.PlacePiece(
                new King(PlayerColor.White, new Vector2Int(4, 0)),
                new Vector2Int(4, 0)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.White, new Vector2Int(5, 0)),
                new Vector2Int(5, 0)
            );
            board.PlacePiece(
                new Knight(PlayerColor.White, new Vector2Int(6, 0)),
                new Vector2Int(6, 0)
            );
            board.PlacePiece(
                new Rook(PlayerColor.White, new Vector2Int(7, 0)),
                new Vector2Int(7, 0)
            );

            for (int x = 0; x < 8; x++)
            {
                board.PlacePiece(
                    new Pawn(PlayerColor.White, new Vector2Int(x, 1)),
                    new Vector2Int(x, 1)
                );
            }

            // Black pieces
            board.PlacePiece(
                new Rook(PlayerColor.Black, new Vector2Int(0, 7)),
                new Vector2Int(0, 7)
            );
            board.PlacePiece(
                new Knight(PlayerColor.Black, new Vector2Int(1, 7)),
                new Vector2Int(1, 7)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.Black, new Vector2Int(2, 7)),
                new Vector2Int(2, 7)
            );
            board.PlacePiece(
                new Queen(PlayerColor.Black, new Vector2Int(3, 7)),
                new Vector2Int(3, 7)
            );
            board.PlacePiece(
                new King(PlayerColor.Black, new Vector2Int(4, 7)),
                new Vector2Int(4, 7)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.Black, new Vector2Int(5, 7)),
                new Vector2Int(5, 7)
            );
            board.PlacePiece(
                new Knight(PlayerColor.Black, new Vector2Int(6, 7)),
                new Vector2Int(6, 7)
            );
            board.PlacePiece(
                new Rook(PlayerColor.Black, new Vector2Int(7, 7)),
                new Vector2Int(7, 7)
            );

            for (int x = 0; x < 8; x++)
            {
                board.PlacePiece(
                    new Pawn(PlayerColor.Black, new Vector2Int(x, 6)),
                    new Vector2Int(x, 6)
                );
            }

            return board;
        }

        public IRuleSet GetRuleSet()
        {
            return new StandardChessRuleSet();
        }
    }
}
