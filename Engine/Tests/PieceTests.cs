using RogueChess.Engine.Board;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Tests for Step 5: Piece system with decorators and status effects.
    /// Verifies that pieces generate pseudo-legal moves and emit candidate events.
    /// </summary>
    public class PieceTests
    {
        [Fact]
        public void Pawn_GetPseudoLegalMoves_GeneratesCorrectMoves()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(pawn, new Vector2Int(1, 1));

            // Act
            var moves = pawn.GetPseudoLegalMoves(state).ToList();

            // Assert
            Assert.Equal(2, moves.Count); // Forward one and forward two from start position
            Assert.Contains(moves, m => m.To == new Vector2Int(1, 2));
            Assert.Contains(moves, m => m.To == new Vector2Int(1, 3));
        }

        [Fact]
        public void Pawn_GetPseudoLegalMoves_WithEnemyPiece_GeneratesCaptureMoves()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var enemyPawn = new Pawn(PlayerColor.Black, new Vector2Int(2, 2));
            
            board.PlacePiece(pawn, new Vector2Int(1, 1));
            board.PlacePiece(enemyPawn, new Vector2Int(2, 2));

            // Act
            var moves = pawn.GetPseudoLegalMoves(state).ToList();

            // Assert
            Assert.Contains(moves, m => m.To == new Vector2Int(2, 2) && m.IsCapture);
        }

        [Fact]
        public void Rook_GetPseudoLegalMoves_GeneratesStraightLineMoves()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var rook = new Rook(PlayerColor.White, new Vector2Int(3, 3));
            board.PlacePiece(rook, new Vector2Int(3, 3));

            // Act
            var moves = rook.GetPseudoLegalMoves(state).ToList();

            // Assert
            Assert.True(moves.Count > 10); // Should have many moves in all directions
            Assert.Contains(moves, m => m.To == new Vector2Int(3, 7)); // Up
            Assert.Contains(moves, m => m.To == new Vector2Int(7, 3)); // Right
            Assert.Contains(moves, m => m.To == new Vector2Int(3, 0)); // Down
            Assert.Contains(moves, m => m.To == new Vector2Int(0, 3)); // Left
        }

        [Fact]
        public void King_GetPseudoLegalMoves_GeneratesOneSquareMoves()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var king = new King(PlayerColor.White, new Vector2Int(3, 3));
            board.PlacePiece(king, new Vector2Int(3, 3));

            // Act
            var moves = king.GetPseudoLegalMoves(state).ToList();

            // Assert
            Assert.Equal(8, moves.Count); // Should have exactly 8 moves (all directions)
        }

        [Fact]
        public void Piece_OnMove_ReturnsNoEventsByDefault()
        {
            // Arrange
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var state = CreateTestGameState();
            var move = new Move(new Vector2Int(1, 1), new Vector2Int(1, 2), pawn);

            // Act
            var events = pawn.OnMove(move, state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void Piece_OnCapture_ReturnsNoEventsByDefault()
        {
            // Arrange
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var state = CreateTestGameState();

            // Act
            var events = pawn.OnCapture(state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void ExplodingDecorator_OnCapture_EmitsTileChangeAndStatusEvents()
        {
            // Arrange
            var innerPiece = new Rook(PlayerColor.White, new Vector2Int(3, 3));
            var explodingRook = new ExplodingDecorator(innerPiece);
            var state = CreateTestGameState();
            state.Board.PlacePiece(explodingRook, new Vector2Int(3, 3));
            
            // Place a piece in an adjacent position to test status effect application
            var targetPiece = new Pawn(PlayerColor.Black, new Vector2Int(2, 2));
            state.Board.PlacePiece(targetPiece, new Vector2Int(2, 2));

            // Act
            var events = explodingRook.OnCapture(state).ToList();

            // Assert
            Assert.True(events.Count > 0);
            Assert.Contains(events, e => e.Type == GameEventType.TileEffectTriggered);
            Assert.Contains(events, e => e.Type == GameEventType.StatusEffectTriggered);
        }

        [Fact]
        public void StatusEffectDecorator_AddStatus_StoresStatusEffect()
        {
            // Arrange
            var innerPiece = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var statusDecorator = new StatusEffectDecorator(innerPiece);
            var burningStatus = new BurningStatus();

            // Act
            statusDecorator.AddStatus(burningStatus);

            // Assert
            var statuses = statusDecorator.GetStatuses().ToList();
            Assert.Single(statuses);
            Assert.Same(burningStatus, statuses[0]);
        }

        [Fact]
        public void StatusEffectDecorator_OnMove_ProcessesStatusEffects()
        {
            // Arrange
            var innerPiece = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var statusDecorator = new StatusEffectDecorator(innerPiece);
            var burningStatus = new BurningStatus();
            statusDecorator.AddStatus(burningStatus);
            
            var state = CreateTestGameState();
            var move = new Move(new Vector2Int(1, 1), new Vector2Int(1, 2), statusDecorator);

            // Act
            var events = statusDecorator.OnMove(move, state).ToList();

            // Assert
            Assert.NotEmpty(events);
            Assert.Contains(events, e => e.Type == GameEventType.StatusEffectTriggered);
        }

        [Fact]
        public void Piece_Clone_CreatesIndependentCopy()
        {
            // Arrange
            var originalPawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));

            // Act
            var clonedPawn = (Pawn)originalPawn.Clone();

            // Assert
            Assert.NotSame(originalPawn, clonedPawn);
            Assert.Equal(originalPawn.Name, clonedPawn.Name);
            Assert.Equal(originalPawn.Owner, clonedPawn.Owner);
            Assert.Equal(originalPawn.Position, clonedPawn.Position);
        }

        [Fact]
        public void ExplodingDecorator_Clone_CreatesIndependentCopy()
        {
            // Arrange
            var innerPiece = new Rook(PlayerColor.White, new Vector2Int(3, 3));
            var originalDecorator = new ExplodingDecorator(innerPiece);

            // Act
            var clonedDecorator = (ExplodingDecorator)originalDecorator.Clone();

            // Assert
            Assert.NotSame(originalDecorator, clonedDecorator);
            Assert.NotSame(originalDecorator.Inner, clonedDecorator.Inner);
            Assert.Equal(originalDecorator.Name, clonedDecorator.Name);
        }

        [Fact]
        public void Piece_GetValue_ReturnsCorrectValues()
        {
            // Arrange & Act & Assert
            Assert.Equal(1, new Pawn(PlayerColor.White, new Vector2Int(0, 0)).GetValue());
            Assert.Equal(5, new Rook(PlayerColor.White, new Vector2Int(0, 0)).GetValue());
            Assert.Equal(100, new King(PlayerColor.White, new Vector2Int(0, 0)).GetValue());
        }

        [Fact]
        public void StatusEffectDecorator_GetValue_ModifiesValueBasedOnStatus()
        {
            // Arrange
            var innerPiece = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            var statusDecorator = new StatusEffectDecorator(innerPiece);
            var burningStatus = new BurningStatus();
            statusDecorator.AddStatus(burningStatus);

            // Act
            var value = statusDecorator.GetValue();

            // Assert
            Assert.True(value < innerPiece.GetValue()); // Burning should reduce value
        }

        private GameState CreateTestGameState()
        {
            var board = new RogueChess.Engine.Board.Board(8, 8);
            return GameState.CreateInitial(board, PlayerColor.White);
        }
    }
}
