using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
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
            "Each player starts with 10 points to randomly draft pieces, then places them on their side of the board";

        public IBoard SetupBoard()
        {
            var board = new RogueChess.Engine.Board.Board(BoardSize, BoardSize);

            // Draft pieces for both players
            var whitePieces = DraftPiecesForPlayer(PlayerColor.White);
            var blackPieces = DraftPiecesForPlayer(PlayerColor.Black);

            // Place pieces on the board following placement rules
            PlacePiecesOnBoard(board, whitePieces, PlayerColor.White);
            PlacePiecesOnBoard(board, blackPieces, PlayerColor.Black);

            return board;
        }

        public IRuleSet GetRuleSet()
        {
            return new LastPieceStandingRuleSet();
        }

        private List<IPiece> DraftPiecesForPlayer(PlayerColor color)
        {
            var pieces = new List<IPiece>();

            // Always add a King for free (not counting against the 10 points)
            var king = new King(color, new Vector2Int(0, 0)); // Position will be set later
            pieces.Add(king);

            // Always add exactly 3 pawns
            for (int i = 0; i < 3; i++)
            {
                var pawn = new Pawn(color, new Vector2Int(0, 0)); // Position will be set later
                pieces.Add(pawn);
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
            return (IPiece)Activator.CreateInstance(pieceType, color, position)!;
        }

        private int GetPieceValue(Type pieceType)
        {
            // Create a temporary instance to get the value
            var tempPiece = CreatePiece(pieceType, PlayerColor.White, new Vector2Int(0, 0));
            return tempPiece.GetValue();
        }

        private void PlacePiecesOnBoard(IBoard board, List<IPiece> pieces, PlayerColor color)
        {
            var isWhite = color == PlayerColor.White;
            var playerSide = isWhite ? 0 : BoardSize - 1;
            var opponentSide = isWhite ? BoardSize - 1 : 0;

            // Separate pieces by type
            var king = pieces.FirstOrDefault(p => p is King);
            var pawns = pieces.Where(p => p is Pawn).ToList();
            var otherPieces = pieces.Where(p => !(p is Pawn) && !(p is King)).ToList();

            // Place pawns closer to the opponent (further from player's starting side)
            var pawnRow = isWhite ? 1 : BoardSize - 2; // White pawns at row 1, Black pawns at row 4
            var otherPiecesRow = isWhite ? 0 : BoardSize - 1; // White pieces at row 0, Black pieces at row 5

            // Get available positions for each row
            var pawnPositions = GetAvailablePositions(board, pawnRow);
            var otherPiecesPositions = GetAvailablePositions(board, otherPiecesRow);

            // Place pawns first
            foreach (var pawn in pawns)
            {
                if (pawnPositions.Count == 0) break;
                var pos = pawnPositions[_random.Next(pawnPositions.Count)];
                pawn.Position = pos;
                board.PlacePiece(pawn, pos);
                pawnPositions.Remove(pos);
            }

            // Place other pieces (but not King) first
            foreach (var piece in otherPieces)
            {
                if (otherPiecesPositions.Count == 0) break;
                var pos = otherPiecesPositions[_random.Next(otherPiecesPositions.Count)];
                piece.Position = pos;
                board.PlacePiece(piece, pos);
                otherPiecesPositions.Remove(pos);
            }

            // Place King last, so it can see all other pieces for positioning
            if (king != null)
            {
                var kingPos = GetBestPositionForKing(board, otherPiecesPositions, color, pawns, otherPieces);
                king.Position = kingPos;
                board.PlacePiece(king, kingPos);
                otherPiecesPositions.Remove(kingPos);
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

        private Vector2Int GetBestPositionForKing(IBoard board, List<Vector2Int> availablePositions, PlayerColor color, List<IPiece> pawns, List<IPiece> otherPieces)
        {
            if (availablePositions.Count == 0)
                return new Vector2Int(BoardSize / 2, color == PlayerColor.White ? 0 : BoardSize - 1);

            var isWhite = color == PlayerColor.White;
            var pawnRow = isWhite ? 1 : BoardSize - 2;
            
            // Find positions where King has a pawn in front
            var validPositions = new List<Vector2Int>();
            
            foreach (var pos in availablePositions)
            {
                // Check if there's a pawn directly in front of this position
                var pawnInFront = pawns.Any(pawn => pawn.Position.X == pos.X && pawn.Position.Y == pawnRow);
                if (pawnInFront)
                {
                    validPositions.Add(pos);
                }
            }

            // If no positions have pawns in front, use all available positions
            if (validPositions.Count == 0)
            {
                validPositions = availablePositions.ToList();
            }

            // Now find the best position among valid positions
            var bestPosition = validPositions[0];
            var bestScore = int.MinValue;

            foreach (var pos in validPositions)
            {
                var score = CalculateKingPositionScore(board, pos, color);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = pos;
                }
            }

            return bestPosition;
        }

        private int CalculateKingPositionScore(IBoard board, Vector2Int kingPos, PlayerColor color)
        {
            var score = 0;
            var isWhite = color == PlayerColor.White;
            var pawnRow = isWhite ? 1 : BoardSize - 2;

            // Check for pawn in front (highest priority)
            var pawnInFront = false;
            for (int x = 0; x < BoardSize; x++)
            {
                var pos = new Vector2Int(x, pawnRow);
                var piece = board.GetPieceAt(pos);
                if (piece != null && piece.Owner == color && piece is Pawn && x == kingPos.X)
                {
                    pawnInFront = true;
                    break;
                }
            }
            
            if (pawnInFront)
            {
                score += 1000; // Very high score for having a pawn in front
            }

            // Count pieces on left and right sides
            var leftPieces = 0;
            var rightPieces = 0;

            for (int x = 0; x < BoardSize; x++)
            {
                var pos = new Vector2Int(x, kingPos.Y);
                var piece = board.GetPieceAt(pos);
                if (piece != null && piece.Owner == color && !(piece is King))
                {
                    if (x < kingPos.X)
                        leftPieces++;
                    else if (x > kingPos.X)
                        rightPieces++;
                }
            }

            // Bonus for having pieces on both sides
            if (leftPieces > 0 && rightPieces > 0)
            {
                score += 500; // High score for having pieces on both sides
            }

            // Bonus for balanced distribution (if there are 2+ pieces on one side)
            if (leftPieces >= 2 || rightPieces >= 2)
            {
                var balance = Math.Abs(leftPieces - rightPieces);
                score += Math.Max(0, 200 - balance * 50); // Better balance = higher score
            }

            // Small bonus for being closer to center
            var centerDistance = Math.Abs(kingPos.X - BoardSize / 2);
            score += Math.Max(0, 50 - centerDistance * 10);

            return score;
        }
    }
}
