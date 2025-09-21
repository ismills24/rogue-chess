using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.Tiles;
using RogueChess.Engine.GameModes.PiecePlacementInit;

namespace RogueChess.Engine.GameModes
{
    /// <summary>
    /// Random chess game mode with random board size, pieces, decorators, and special tiles
    /// </summary>
    public class RandomChessMode : IGameMode
    {
        private readonly Random _random = new Random();

        public string Name => "Random Chess";
        public string Description =>
            "Chaotic chess with random board size, pieces, decorators, and special tiles";

        public RogueChess.Engine.GameModes.PiecePlacementInit.PiecePlacementInit GetPiecePlacementInit()
        {
            return new RandomChessPlacement();
        }

        public IBoard SetupBoard()
        {
            // Get the placement init to determine board size
            var placementInit = GetPiecePlacementInit();
            var board = new RogueChess.Engine.Board.Board(placementInit.BoardWidth, placementInit.BoardHeight);

            // Set up random special tiles
            SetupRandomTiles(board);

            // Place pieces using the placement init
            placementInit.PlacePieces(board, PlayerColor.White);
            placementInit.PlacePieces(board, PlayerColor.Black);

            return board;
        }


        private void SetupRandomTiles(IBoard board)
        {
            var tileTypes = new Type[]
            {
                typeof(ScorchedTile),
                typeof(SlipperyTile),
                typeof(GuardianTile),
            };

            // Place random special tiles (about 20% of the board)
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (_random.NextDouble() < 0.2) // 20% chance
                    {
                        var tileType = tileTypes[_random.Next(tileTypes.Length)];
                        var tile = (ITile)Activator.CreateInstance(tileType)!;
                        board.SetTile(new Vector2Int(x, y), tile);
                    }
                }
            }
        }



        public IRuleSet GetRuleSet()
        {
            return new LastPieceStandingRuleSet(); // Use last piece standing for random games
        }
    }
}



