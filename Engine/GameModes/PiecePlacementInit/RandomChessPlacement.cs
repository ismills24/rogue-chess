using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine.GameModes.PiecePlacementInit
{
    /// <summary>
    /// Random chess piece placement with random pieces, decorators, and positions
    /// </summary>
    public class RandomChessPlacement : PiecePlacementInit
    {
        private readonly Random _random = new Random();
        private int _boardWidth;
        private int _boardHeight;

        public RandomChessPlacement()
            : base(0, 0)
        {
            // Random board size between 4x4 and 8x8
            _boardWidth = _random.Next(4, 9); // 4 to 8
            _boardHeight = _boardWidth;
            BoardWidth = _boardWidth;
            BoardHeight = _boardHeight;
        }

        public override IBoard PlacePieces(IBoard board, PlayerColor color)
        {
            // Generate random pieces for the specified color
            var pieces = GenerateRandomPieces(color);

            // Place pieces randomly
            PlaceRandomPiecesForColor(board, pieces, color);

            return board;
        }

        private List<IPiece> GenerateRandomPieces(PlayerColor color)
        {
            var pieces = new List<IPiece>();
            var pieceTypes = new Type[]
            {
                typeof(Pawn),
                typeof(Rook),
                typeof(Knight),
                typeof(Bishop),
                typeof(Queen),
            };

            // Calculate how many pieces to place (max 50% of board, but at least 2 per side)
            var totalSquares = BoardWidth * BoardHeight;
            var maxPiecesPerSide = Math.Min(totalSquares / 4, BoardWidth * 2); // Max 50% of board, but reasonable limit
            var piecesPerSide = Math.Max(2, _random.Next(2, maxPiecesPerSide + 1));

            for (int i = 0; i < piecesPerSide; i++)
            {
                // For the first piece of each color, always place a King
                IPiece piece;
                if (i == 0)
                {
                    piece = new King(color, new Vector2Int(0, 0));
                }
                else
                {
                    // Random piece type for other pieces
                    var pieceType = pieceTypes[_random.Next(pieceTypes.Length)];
                    piece = (IPiece)
                        Activator.CreateInstance(pieceType, color, new Vector2Int(0, 0))!;
                }

                // Add random decorators
                piece = new ScapegoatDecorator(piece);
                pieces.Add(piece);
            }

            return pieces;
        }

        private void PlaceRandomPiecesForColor(IBoard board, List<IPiece> pieces, PlayerColor color)
        {
            var placedPieces = 0;
            var attempts = 0;
            var maxAttempts = BoardWidth * BoardHeight * 2; // Prevent infinite loops

            while (placedPieces < pieces.Count && attempts < maxAttempts)
            {
                var x = _random.Next(BoardWidth);
                var y = _random.Next(BoardHeight);
                var pos = new Vector2Int(x, y);

                // Skip if position is already occupied
                if (board.GetPieceAt(pos) != null)
                {
                    attempts++;
                    continue;
                }

                // Place the piece
                var piece = pieces[placedPieces];
                piece.Position = pos;
                board.PlacePiece(piece, pos);
                placedPieces++;
                attempts++;
            }
        }
    }
}
