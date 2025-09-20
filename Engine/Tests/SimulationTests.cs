using RogueChess.Engine.Board;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Tests for Step 11: Player Controllers & AI Sandbox.
    /// Verifies that simulation API works correctly and is isolated from canonical pipeline.
    /// </summary>
    public class SimulationTests
    {
        [Fact]
        public void GameState_Simulate_WithLegalMove_ReturnsNewState()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(pawn, new Vector2Int(1, 1));
            
            var move = new Move(new Vector2Int(1, 1), new Vector2Int(1, 2), pawn);

            // Act
            var simulatedState = state.Simulate(move, ruleset);

            // Assert
            Assert.NotNull(simulatedState);
            Assert.NotSame(state, simulatedState);
            var simulatedPiece = simulatedState.Board.GetPieceAt(new Vector2Int(1, 2));
            Assert.NotNull(simulatedPiece);
            Assert.Equal(pawn.Name, simulatedPiece.Name);
            Assert.Equal(pawn.Owner, simulatedPiece.Owner);
            Assert.Equal(new Vector2Int(1, 2), simulatedPiece.Position);
            Assert.Null(simulatedState.Board.GetPieceAt(new Vector2Int(1, 1)));
        }

        [Fact]
        public void GameState_Simulate_WithIllegalMove_ReturnsNull()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(pawn, new Vector2Int(1, 1));
            
            var illegalMove = new Move(new Vector2Int(1, 1), new Vector2Int(1, 4), pawn); // Pawn can't move 3 squares

            // Act
            var simulatedState = state.Simulate(illegalMove, ruleset);

            // Assert
            Assert.Null(simulatedState);
        }

        [Fact]
        public void GameState_Simulate_WithCapture_RemovesCapturedPiece()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var whitePawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var blackPawn = new Pawn(PlayerColor.Black, new Vector2Int(2, 2));
            board.PlacePiece(whitePawn, new Vector2Int(1, 1));
            board.PlacePiece(blackPawn, new Vector2Int(2, 2));
            
            var captureMove = new Move(new Vector2Int(1, 1), new Vector2Int(2, 2), whitePawn, true);

            // Act
            var simulatedState = state.Simulate(captureMove, ruleset);

            // Assert
            Assert.NotNull(simulatedState);
            var simulatedPiece = simulatedState.Board.GetPieceAt(new Vector2Int(2, 2));
            Assert.NotNull(simulatedPiece);
            Assert.Equal(whitePawn.Name, simulatedPiece.Name);
            Assert.Equal(whitePawn.Owner, simulatedPiece.Owner);
            Assert.Null(simulatedState.Board.GetPieceAt(new Vector2Int(1, 1)));
            // Black pawn should be captured (no piece at original position)
        }

        [Fact]
        public void GameState_Simulate_DoesNotModifyOriginalState()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(pawn, new Vector2Int(1, 1));
            
            var move = new Move(new Vector2Int(1, 1), new Vector2Int(1, 2), pawn);

            // Act
            var simulatedState = state.Simulate(move, ruleset);

            // Assert
            Assert.NotNull(simulatedState);
            // Original state should be unchanged
            Assert.Equal(pawn, state.Board.GetPieceAt(new Vector2Int(1, 1)));
            Assert.Null(state.Board.GetPieceAt(new Vector2Int(1, 2)));
        }

        [Fact]
        public void GameState_Evaluate_WithWhiteAdvantage_ReturnsPositiveScore()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            // White has more valuable pieces
            var whiteQueen = new Queen(PlayerColor.White, new Vector2Int(0, 0));
            var blackPawn = new Pawn(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whiteQueen, new Vector2Int(0, 0));
            board.PlacePiece(blackPawn, new Vector2Int(7, 7));

            // Act
            var score = state.Evaluate();

            // Assert
            Assert.True(score > 0); // White should have advantage
        }

        [Fact]
        public void GameState_Evaluate_WithBlackAdvantage_ReturnsNegativeScore()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            // Black has more valuable pieces
            var whitePawn = new Pawn(PlayerColor.White, new Vector2Int(0, 0));
            var blackQueen = new Queen(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whitePawn, new Vector2Int(0, 0));
            board.PlacePiece(blackQueen, new Vector2Int(7, 7));

            // Act
            var score = state.Evaluate();

            // Assert
            Assert.True(score < 0); // Black should have advantage
        }

        [Fact]
        public void GameState_GetAllLegalMoves_ReturnsAllMovesForCurrentPlayer()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var whitePawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var blackPawn = new Pawn(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whitePawn, new Vector2Int(1, 1));
            board.PlacePiece(blackPawn, new Vector2Int(7, 7));

            // Act
            var legalMoves = state.GetAllLegalMoves(ruleset).ToList();

            // Assert
            Assert.NotEmpty(legalMoves);
            Assert.All(legalMoves, move => Assert.Equal(PlayerColor.White, move.Piece.Owner));
        }

        [Fact]
        public void SimulationAIController_SelectMove_ReturnsBestMove()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var aiController = new SimulationAIController(ruleset, 1);
            
            var whitePawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var blackPawn = new Pawn(PlayerColor.Black, new Vector2Int(2, 2));
            board.PlacePiece(whitePawn, new Vector2Int(1, 1));
            board.PlacePiece(blackPawn, new Vector2Int(2, 2));

            // Act
            var selectedMove = aiController.SelectMove(state);

            // Assert
            Assert.NotNull(selectedMove);
            Assert.Equal(PlayerColor.White, selectedMove.Piece.Owner);
        }

        [Fact]
        public void SimulationAIController_QuickEvaluate_ReturnsPositionScore()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var aiController = new SimulationAIController(ruleset);
            
            var whiteQueen = new Queen(PlayerColor.White, new Vector2Int(0, 0));
            board.PlacePiece(whiteQueen, new Vector2Int(0, 0));

            // Act
            var score = aiController.QuickEvaluate(state);

            // Assert
            Assert.True(score > 0); // White should have advantage
        }

        [Fact]
        public void SimulationAIController_EvaluateAllMoves_ReturnsMoveScores()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var aiController = new SimulationAIController(ruleset);
            
            var whitePawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(whitePawn, new Vector2Int(1, 1));

            // Act
            var moveScores = aiController.EvaluateAllMoves(state).ToList();

            // Assert
            Assert.NotEmpty(moveScores);
            Assert.All(moveScores, ms => Assert.NotNull(ms.Move));
        }

        [Fact]
        public void Simulation_Isolation_DoesNotAffectCanonicalHistory()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(pawn, new Vector2Int(1, 1));
            
            var move = new Move(new Vector2Int(1, 1), new Vector2Int(1, 2), pawn);
            var originalMoveCount = state.MoveHistory.Count;

            // Act
            var simulatedState = state.Simulate(move, ruleset);

            // Assert
            Assert.NotNull(simulatedState);
            Assert.Equal(originalMoveCount, state.MoveHistory.Count); // Original state unchanged
            Assert.Equal(originalMoveCount + 1, simulatedState.MoveHistory.Count); // Simulated state has the move
        }

        [Fact]
        public void Simulation_DeepCloning_CreatesIndependentState()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(pawn, new Vector2Int(1, 1));
            
            var move = new Move(new Vector2Int(1, 1), new Vector2Int(1, 2), pawn);

            // Act
            var simulatedState = state.Simulate(move, ruleset);

            // Assert
            Assert.NotNull(simulatedState);
            // The simulated state should be independent from the original
            var simulatedPiece = simulatedState.Board.GetPieceAt(new Vector2Int(1, 2));
            Assert.NotNull(simulatedPiece);
            Assert.Equal(pawn.Name, simulatedPiece.Name);
            Assert.Equal(pawn.Owner, simulatedPiece.Owner);
            // Original state should be unchanged
            Assert.Equal(pawn, state.Board.GetPieceAt(new Vector2Int(1, 1)));
        }

        [Fact]
        public void Simulation_WithStatusEffects_PreservesDecorators()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var statusDecorator = new StatusEffectDecorator(pawn);
            board.PlacePiece(statusDecorator, new Vector2Int(1, 1));
            
            // Get all legal moves for the decorator to see what's available
            var legalMoves = ruleset.GetLegalMoves(state, statusDecorator).ToList();
            Assert.NotEmpty(legalMoves); // Should have at least one legal move
            
            var move = legalMoves.First(); // Use the first available legal move

            // Act
            var simulatedState = state.Simulate(move, ruleset);

            // Assert
            Assert.NotNull(simulatedState);
            var simulatedPiece = simulatedState.Board.GetPieceAt(move.To);
            Assert.NotNull(simulatedPiece);
            Assert.IsType<StatusEffectDecorator>(simulatedPiece);
        }
    }
}
