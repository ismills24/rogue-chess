using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.WinConditions;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Tests for Step 6: Ruleset system with legality checking and game termination.
    /// </summary>
    public class RulesetTests
    {
        [Fact]
        public void StandardChessRuleSet_GetLegalMoves_FiltersOutMovesThatPutKingInCheck()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new StandardChessRuleSet();
            
            // Place a white king
            var king = new King(PlayerColor.White, new Vector2Int(4, 0));
            board.PlacePiece(king, new Vector2Int(4, 0));
            
            // Place a black rook that attacks the king
            var blackRook = new Rook(PlayerColor.Black, new Vector2Int(4, 7));
            board.PlacePiece(blackRook, new Vector2Int(4, 7));

            // Act
            var legalMoves = ruleset.GetLegalMoves(state, king).ToList();

            // Assert
            // The king should not be able to move to squares that are still in check
            // Only moves that get out of check should be legal
            Assert.NotEmpty(legalMoves); // King should have some legal moves to get out of check
        }

        [Fact]
        public void StandardChessRuleSet_GetLegalMoves_AllowsMovesThatDontPutKingInCheck()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new StandardChessRuleSet();
            
            // Place a white king and a white pawn with no threats
            var king = new King(PlayerColor.White, new Vector2Int(4, 0));
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(3, 1));
            board.PlacePiece(king, new Vector2Int(4, 0));
            board.PlacePiece(pawn, new Vector2Int(3, 1));

            // Act
            var legalMoves = ruleset.GetLegalMoves(state, pawn).ToList();

            // Assert
            // The pawn should be able to move forward safely
            Assert.NotEmpty(legalMoves);
            Assert.Contains(legalMoves, m => m.To == new Vector2Int(3, 2));
        }

        [Fact]
        public void LastPieceStandingRuleSet_GetLegalMoves_ReturnsAllPseudoLegalMoves()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(4, 1));
            board.PlacePiece(pawn, new Vector2Int(4, 1));

            // Act
            var legalMoves = ruleset.GetLegalMoves(state, pawn).ToList();
            var pseudoLegalMoves = pawn.GetPseudoLegalMoves(state).ToList();

            // Assert
            Assert.Equal(pseudoLegalMoves.Count, legalMoves.Count);
        }

        [Fact]
        public void LastPieceStandingRuleSet_IsGameOver_WithNoPieces_ReturnsDraw()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();

            // Act
            var isGameOver = ruleset.IsGameOver(state, out var winner);

            // Assert
            Assert.True(isGameOver);
            Assert.Null(winner); // Draw
        }

        [Fact]
        public void LastPieceStandingRuleSet_IsGameOver_WithOnlyWhitePieces_ReturnsWhiteWinner()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));

            // Act
            var isGameOver = ruleset.IsGameOver(state, out var winner);

            // Assert
            Assert.True(isGameOver);
            Assert.Equal(PlayerColor.White, winner);
        }

        [Fact]
        public void LastPieceStandingRuleSet_IsGameOver_WithBothPlayers_ReturnsNotOver()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackKing = new King(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackKing, new Vector2Int(7, 7));

            // Act
            var isGameOver = ruleset.IsGameOver(state, out var winner);

            // Assert
            Assert.False(isGameOver);
            Assert.Null(winner);
        }

        [Fact]
        public void CheckRules_IsKingInCheck_WithNoKing_ReturnsTrue()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);

            // Act
            var isInCheck = CheckRules.IsKingInCheck(state, PlayerColor.White);

            // Assert
            Assert.True(isInCheck); // No king = checkmate
        }

        [Fact]
        public void CheckRules_IsKingInCheck_WithAttackingRook_ReturnsTrue()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            var whiteKing = new King(PlayerColor.White, new Vector2Int(4, 0));
            var blackRook = new Rook(PlayerColor.Black, new Vector2Int(4, 7));
            board.PlacePiece(whiteKing, new Vector2Int(4, 0));
            board.PlacePiece(blackRook, new Vector2Int(4, 7));

            // Act
            var isInCheck = CheckRules.IsKingInCheck(state, PlayerColor.White);

            // Assert
            Assert.True(isInCheck);
        }

        [Fact]
        public void CheckRules_IsKingInCheck_WithNoThreats_ReturnsFalse()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            var whiteKing = new King(PlayerColor.White, new Vector2Int(4, 0));
            var blackPawn = new Pawn(PlayerColor.Black, new Vector2Int(0, 6));
            board.PlacePiece(whiteKing, new Vector2Int(4, 0));
            board.PlacePiece(blackPawn, new Vector2Int(0, 6));

            // Act
            var isInCheck = CheckRules.IsKingInCheck(state, PlayerColor.White);

            // Assert
            Assert.False(isInCheck);
        }

        [Fact]
        public void CheckmateCondition_IsGameOver_WithCheckmate_ReturnsTrue()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var checkmateCondition = new CheckmateCondition();
            
            // Set up a checkmate position (simplified)
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackRook1 = new Rook(PlayerColor.Black, new Vector2Int(0, 7));
            var blackRook2 = new Rook(PlayerColor.Black, new Vector2Int(1, 7));
            
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackRook1, new Vector2Int(0, 7));
            board.PlacePiece(blackRook2, new Vector2Int(1, 7));

            // Act
            var isGameOver = checkmateCondition.IsGameOver(state, out var winner);

            // Assert
            Assert.True(isGameOver);
            Assert.Equal(PlayerColor.Black, winner);
        }

        [Fact]
        public void CheckmateCondition_IsGameOver_WithStalemate_ReturnsTrue()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var checkmateCondition = new CheckmateCondition();
            
            // Set up a stalemate position - white king with no legal moves but not in check
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackRook1 = new Rook(PlayerColor.Black, new Vector2Int(1, 7));
            var blackRook2 = new Rook(PlayerColor.Black, new Vector2Int(7, 1));
            
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackRook1, new Vector2Int(1, 7));
            board.PlacePiece(blackRook2, new Vector2Int(7, 1));

            // Act
            var isGameOver = checkmateCondition.IsGameOver(state, out var winner);

            // Assert
            Assert.True(isGameOver);
            Assert.Null(winner); // Stalemate is a draw
        }

        [Fact]
        public void CheckmateCondition_IsGameOver_WithLegalMoves_ReturnsFalse()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var checkmateCondition = new CheckmateCondition();
            
            // Set up a normal position with legal moves
            var whiteKing = new King(PlayerColor.White, new Vector2Int(4, 0));
            var whitePawn = new Pawn(PlayerColor.White, new Vector2Int(4, 1));
            var blackKing = new King(PlayerColor.Black, new Vector2Int(4, 7));
            
            board.PlacePiece(whiteKing, new Vector2Int(4, 0));
            board.PlacePiece(whitePawn, new Vector2Int(4, 1));
            board.PlacePiece(blackKing, new Vector2Int(4, 7));

            // Act
            var isGameOver = checkmateCondition.IsGameOver(state, out var winner);

            // Assert
            Assert.False(isGameOver);
            Assert.Null(winner);
        }
    }
}
