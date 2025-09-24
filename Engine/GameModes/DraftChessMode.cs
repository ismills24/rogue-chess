using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Board;
using RogueChess.Engine.GameModes.PiecePlacementInit;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.GameModes
{
    /// <summary>
    /// Draft chess game mode where each player starts with a fixed amount of points
    /// and randomly selects pieces to spend those points on, then places them on their side
    /// </summary>
    public class DraftChessMode : IGameMode
    {
        private readonly Random _random = new Random();
        private const int StartingPoints = 10;
        private const int BoardSize = 6;

        public string Name => "Draft Chess";
        public string Description =>
            "Each player starts with 10 points to randomly draft pieces, then places them on their side of the board. All pieces have ranged capture ability!";

        public RogueChess.Engine.GameModes.PiecePlacementInit.PiecePlacementInit GetPiecePlacementInit()
        {
            return new DraftChessPlacement();
        }

        public IBoard SetupBoard()
        {
            var board = new RogueChess.Engine.Board.Board(BoardSize, BoardSize);

            // Place pieces using the placement init
            var placementInit = GetPiecePlacementInit();
            placementInit.PlacePieces(board, PlayerColor.White);
            placementInit.PlacePieces(board, PlayerColor.Black);

            // Decorate all pieces as Marksmen
            foreach (var tile in board.GetAllTiles())
            {
                var piece = board.GetPieceAt(tile.Position);
                if (piece != null)
                {
                    board.RemovePiece(tile.Position);
                    board.PlacePiece(new MarksmanDecorator(piece), tile.Position);
                }
            }

            return board;
        }

        public IRuleSet GetRuleSet()
        {
            return new LastPieceStandingRuleSet();
        }
    }
}
