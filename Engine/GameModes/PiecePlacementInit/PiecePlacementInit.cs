using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.GameModes.PiecePlacementInit
{
    /// <summary>
    /// Abstract base class for piece placement initialization
    /// </summary>
    public abstract class PiecePlacementInit
    {
        /// <summary>
        /// Board width for this placement configuration
        /// </summary>
        public int BoardWidth { get; protected set; }

        /// <summary>
        /// Board height for this placement configuration
        /// </summary>
        public int BoardHeight { get; protected set; }

        protected PiecePlacementInit(int boardWidth, int boardHeight)
        {
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
        }

        /// <summary>
        /// Abstract method that determines how pieces are initially placed on the board for a specific color
        /// </summary>
        /// <param name="board">The board to place pieces on</param>
        /// <param name="color">The color of pieces to place</param>
        /// <returns>The board with pieces placed</returns>
        public abstract IBoard PlacePieces(IBoard board, PlayerColor color);
    }
}



