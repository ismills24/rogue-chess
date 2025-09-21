using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.StatusEffects;
using RogueChess.Engine.Tiles;
using RogueChess.Engine.GameModes.PiecePlacementInit;

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

        public RogueChess.Engine.GameModes.PiecePlacementInit.PiecePlacementInit GetPiecePlacementInit()
        {
            return new StandardChessPlacement();
        }

        public IBoard SetupBoard()
        {
            var board = new RogueChess.Engine.Board.Board(8, 8);

            // Place pieces using the placement init
            var placementInit = GetPiecePlacementInit();
            placementInit.PlacePieces(board, PlayerColor.White);
            placementInit.PlacePieces(board, PlayerColor.Black);

            return board;
        }

        public IRuleSet GetRuleSet()
        {
            return new StandardChessRuleSet();
        }
    }
}



