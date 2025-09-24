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
    /// Standard chess piece placement with traditional starting positions
    /// </summary>
    public class StandardChessPlacement : PiecePlacementInit
    {
        public StandardChessPlacement()
            : base(8, 8) { }

        public override IBoard PlacePieces(IBoard board, PlayerColor color)
        {
            // Generate pieces for the specified color
            var pieces = GenerateStandardPieces(color);

            // Place pieces in traditional positions
            PlacePiecesForColor(board, pieces, color);

            return board;
        }

        private List<IPiece> GenerateStandardPieces(PlayerColor color)
        {
            var pieces = new List<IPiece>();

            // Back row pieces
            pieces.Add(new Rook(color, new Vector2Int(0, 0)));
            pieces.Add(new Knight(color, new Vector2Int(0, 0)));
            pieces.Add(new Bishop(color, new Vector2Int(0, 0)));
            pieces.Add(new Queen(color, new Vector2Int(0, 0)));
            pieces.Add(new King(color, new Vector2Int(0, 0)));
            pieces.Add(new Bishop(color, new Vector2Int(0, 0)));
            pieces.Add(new Knight(color, new Vector2Int(0, 0)));
            pieces.Add(new Rook(color, new Vector2Int(0, 0)));

            // Pawns
            for (int x = 0; x < 8; x++)
            {
                pieces.Add(new Pawn(color, new Vector2Int(0, 0)));
            }

            return pieces;
        }

        private void PlacePiecesForColor(IBoard board, List<IPiece> pieces, PlayerColor color)
        {
            if (color == PlayerColor.White)
            {
                // White pieces - traditional starting positions
                var backRowPieces = pieces.Where(p => !(p is Pawn)).ToList();
                var pawns = pieces.Where(p => p is Pawn).ToList();

                // Place back row pieces
                for (int i = 0; i < backRowPieces.Count && i < 8; i++)
                {
                    backRowPieces[i].Position = new Vector2Int(i, 0);
                    board.PlacePiece(backRowPieces[i], new Vector2Int(i, 0));
                }

                // Place pawns
                for (int i = 0; i < pawns.Count && i < 8; i++)
                {
                    pawns[i].Position = new Vector2Int(i, 1);
                    board.PlacePiece(pawns[i], new Vector2Int(i, 1));
                }
            }
            else
            {
                // Black pieces - traditional starting positions
                var backRowPieces = pieces.Where(p => !(p is Pawn)).ToList();
                var pawns = pieces.Where(p => p is Pawn).ToList();

                // Place back row pieces
                for (int i = 0; i < backRowPieces.Count && i < 8; i++)
                {
                    backRowPieces[i].Position = new Vector2Int(i, 7);
                    board.PlacePiece(backRowPieces[i], new Vector2Int(i, 7));
                }

                // Place pawns
                for (int i = 0; i < pawns.Count && i < 8; i++)
                {
                    pawns[i].Position = new Vector2Int(i, 6);
                    board.PlacePiece(pawns[i], new Vector2Int(i, 6));
                }
            }
        }
    }
}
