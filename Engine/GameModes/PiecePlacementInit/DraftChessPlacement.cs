using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.GameModes.PiecePlacementInit
{
    /// <summary>
    /// Draft chess piece placement with random piece selection and strategic positioning
    /// </summary>
    public class DraftChessPlacement : PiecePlacementInit
    {
        private readonly Random _random = new Random();
        private const int BoardSize = 6;

        public DraftChessPlacement() : base(BoardSize, BoardSize) { }

        public override IBoard PlacePieces(IBoard board, PlayerColor color)
        {
            var pieces = DraftPiecesForPlayer(color);
            PlacePiecesOnBoard(board, pieces, color);
            return board;
        }

        private List<IPiece> DraftPiecesForPlayer(PlayerColor color)
        {
            var pieces = new List<IPiece>();

            // Always add a King for free (not counting against the 10 points)
            var king = new King(color, new Vector2Int(0, 0)); // Position will be set later
            pieces.Add(new MarksmanDecorator(king));

            // Always add exactly 3 pawns
            for (int i = 0; i < 3; i++)
            {
                var pawn = new Pawn(color, new Vector2Int(0, 0)); // Position will be set later
                pieces.Add(new MarksmanDecorator(pawn));
            }

            // Add exactly 3 other pieces (randomly selected from available types)
            var pieceTypes = GetAvailablePieceTypes();
            for (int i = 0; i < 3; i++)
            {
                var selectedPiece = pieceTypes[_random.Next(pieceTypes.Length)];
                var piece = CreatePiece(selectedPiece, color, new Vector2Int(0, 0)); // Position will be set later
                pieces.Add(piece);
            }

            return pieces;
        }

        private Type[] GetAvailablePieceTypes()
        {
            return new Type[]
            {
                typeof(Bishop),
                typeof(Knight),
                typeof(Rook)
                // Pawn is excluded - always exactly 3 pawns
                // King is excluded - always added for free
                // Queen is excluded - not available for selection
            };
        }

        private IPiece CreatePiece(Type pieceType, PlayerColor color, Vector2Int position)
        {
            var basePiece = (IPiece)Activator.CreateInstance(pieceType, color, position)!;
            // Wrap every piece with MarksmanDecorator in draft mode
            return new MarksmanDecorator(basePiece);
        }

        private void PlacePiecesOnBoard(IBoard board, List<IPiece> pieces, PlayerColor color)
        {
            var isWhite = color == PlayerColor.White;

            var king = pieces.FirstOrDefault(p => GetInnerPiece(p) is King);
            var pawns = pieces.Where(p => GetInnerPiece(p) is Pawn).ToList();
            var otherPieces = pieces.Where(p => !(GetInnerPiece(p) is Pawn) && !(GetInnerPiece(p) is King)).ToList();
            
            Console.WriteLine($"Player: {color}, King: {king != null}, Pawns: {pawns.Count}, OtherPieces: {otherPieces.Count}");

            var isOddBoard = BoardSize % 2 == 1;
            
            List<int> playerRows;
            if (isOddBoard)
            {
                // For odd boards, each player gets (BoardSize - 1) / 2 rows, ignoring center row
                var rowsPerPlayer = (int)Math.Floor((double)BoardSize / 2);
                playerRows = isWhite ? 
                    Enumerable.Range(0, rowsPerPlayer).ToList() : // White: rows 0 to (rowsPerPlayer-1)
                    Enumerable.Range(rowsPerPlayer + 1, rowsPerPlayer).ToList(); // Black: rows (rowsPerPlayer+1) to (BoardSize-1)
            }
            else
            {
                // For even boards, each player gets BoardSize / 2 rows
                var halfSize = BoardSize / 2;
                playerRows = isWhite ? 
                    Enumerable.Range(0, halfSize).ToList() : // White: rows 0 to (halfSize-1)
                    Enumerable.Range(halfSize, halfSize).ToList(); // Black: rows halfSize to (BoardSize-1)
            }

            List<int> pawnRows, otherPiecesRows;
            
            var halfOfPlayerRows = (playerRows.Count + 1) / 2; // At least half, rounded up
            
            if (isWhite)
            {
                // White pawns go in rows closest to Black (top of their half)
                // For White: higher row numbers are closer to Black
                pawnRows = playerRows.OrderByDescending(r => r).Take(halfOfPlayerRows).ToList();
                otherPiecesRows = playerRows.OrderByDescending(r => r).Skip(halfOfPlayerRows).ToList();
            }
            else
            {
                // Black pawns go in rows closest to White (bottom of their half)
                // For Black: lower row numbers are closer to White
                pawnRows = playerRows.OrderBy(r => r).Take(halfOfPlayerRows).ToList();
                otherPiecesRows = playerRows.OrderBy(r => r).Skip(halfOfPlayerRows).ToList();
            }

            // Debug output
            Console.WriteLine($"=== DRAFT PLACEMENT DEBUG ===");
            Console.WriteLine($"Player: {color}, BoardSize: {BoardSize}, IsOddBoard: {isOddBoard}");
            Console.WriteLine($"PlayerRows: [{string.Join(", ", playerRows)}]");
            Console.WriteLine($"HalfOfPlayerRows: {halfOfPlayerRows}");
            Console.WriteLine($"PawnRows: [{string.Join(", ", pawnRows)}]");
            Console.WriteLine($"OtherPiecesRows: [{string.Join(", ", otherPiecesRows)}]");
            Console.WriteLine($"=============================");

            // Get all available positions for pawns and other pieces
            // Pawns can ONLY be placed in pawn rows
            var pawnPositions = new List<Vector2Int>();
            foreach (var row in pawnRows)
            {
                pawnPositions.AddRange(GetAvailablePositions(board, row));
            }
            
            // Pieces can ONLY be placed in piece rows
            var otherPiecesPositions = new List<Vector2Int>();
            foreach (var row in otherPiecesRows)
            {
                otherPiecesPositions.AddRange(GetAvailablePositions(board, row));
            }
            
            Console.WriteLine($"PawnPositions: {pawnPositions.Count}, OtherPiecesPositions: {otherPiecesPositions.Count}");

            // Place King first in the center of the furthest back row
            if (king != null)
            {
                // King goes in the furthest back row (farthest from opponent)
                var backRow = isWhite ? otherPiecesRows.Min() : otherPiecesRows.Max();
                
                // Calculate center position
                var centerX = BoardSize / 2;
                
                // If the board has an even number of columns, randomly place on one of the middle two tiles
                if (BoardSize % 2 == 0)
                {
                    // For even boards, randomly choose between center-1 and center
                    centerX = _random.Next(2) == 0 ? centerX - 1 : centerX;
                }
                
                var kingPos = new Vector2Int(centerX, backRow);
                king.Position = kingPos;
                board.PlacePiece(king, kingPos);
                
                // Remove the King's position from available positions
                otherPiecesPositions.Remove(kingPos);
                
                Console.WriteLine($"King placed at {kingPos}");
            }

            // Place other pieces randomly across their designated rows
            foreach (var piece in otherPieces)
            {
                if (otherPiecesPositions.Count == 0) break;
                var pos = otherPiecesPositions[_random.Next(otherPiecesPositions.Count)];
                piece.Position = pos;
                board.PlacePiece(piece, pos);
                otherPiecesPositions.Remove(pos);
                Console.WriteLine($"Piece {piece.GetType().Name} placed at {pos}");
            }

            // Place pawns randomly across their designated rows
            foreach (var pawn in pawns)
            {
                if (pawnPositions.Count == 0) break;
                var pos = pawnPositions[_random.Next(pawnPositions.Count)];
                pawn.Position = pos;
                board.PlacePiece(pawn, pos);
                pawnPositions.Remove(pos);
                Console.WriteLine($"Pawn placed at {pos}");
            }
        }

        private List<Vector2Int> GetAvailablePositions(IBoard board, int row)
        {
            var positions = new List<Vector2Int>();
            for (int x = 0; x < BoardSize; x++)
            {
                var pos = new Vector2Int(x, row);
                if (board.GetPieceAt(pos) == null)
                {
                    positions.Add(pos);
                }
            }
            return positions;
        }

        private IPiece GetInnerPiece(IPiece piece)
        {
            // Unwrap decorators to get to the actual piece type
            while (piece is PieceDecoratorBase decorator)
            {
                piece = decorator.Inner;
            }
            return piece;
        }
    }
}
