using RogueChess.Engine.Board;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Tests for Step 4: Tile system with candidate events.
    /// Verifies that tiles emit candidate events only and never mutate the board directly.
    /// </summary>
    public class TileTests
    {
        [Fact]
        public void StandardTile_OnEnter_ReturnsNoEvents()
        {
            // Arrange
            var tile = new StandardTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameState();

            // Act
            var events = tile.OnEnter(piece, new Vector2Int(0, 0), state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void StandardTile_OnTurnStart_ReturnsNoEvents()
        {
            // Arrange
            var tile = new StandardTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameState();

            // Act
            var events = tile.OnTurnStart(piece, new Vector2Int(0, 0), state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void ScorchedTile_OnEnter_EmitsStatusEffectCandidateEvent()
        {
            // Arrange
            var tile = new ScorchedTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameState();

            // Act
            var events = tile.OnEnter(piece, new Vector2Int(0, 0), state).ToList();

            // Assert
            Assert.Single(events);
            var candidateEvent = events[0];
            Assert.Equal(GameEventType.StatusEffectTriggered, candidateEvent.Type);
            Assert.False(candidateEvent.IsPlayerAction);
            Assert.IsType<StatusApplyPayload>(candidateEvent.Payload);
        }

        [Fact]
        public void ScorchedTile_OnTurnStart_EmitsStatusEffectCandidateEvent()
        {
            // Arrange
            var tile = new ScorchedTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameState();

            // Act
            var events = tile.OnTurnStart(piece, new Vector2Int(0, 0), state).ToList();

            // Assert
            Assert.Single(events);
            var candidateEvent = events[0];
            Assert.Equal(GameEventType.StatusEffectTriggered, candidateEvent.Type);
            Assert.False(candidateEvent.IsPlayerAction);
            Assert.IsType<StatusApplyPayload>(candidateEvent.Payload);
        }

        [Fact]
        public void SlipperyTile_OnEnter_WithValidMove_EmitsTileEffectCandidateEvent()
        {
            // Arrange
            var tile = new SlipperyTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameStateWithMove(new Vector2Int(0, 0), new Vector2Int(1, 0));

            // Act
            var events = tile.OnEnter(piece, new Vector2Int(1, 0), state).ToList();

            // Assert
            Assert.Single(events);
            var candidateEvent = events[0];
            Assert.Equal(GameEventType.TileEffectTriggered, candidateEvent.Type);
            Assert.False(candidateEvent.IsPlayerAction);
            Assert.IsType<ForcedSlidePayload>(candidateEvent.Payload);
        }

        [Fact]
        public void SlipperyTile_OnEnter_WithNoMoveHistory_ReturnsNoEvents()
        {
            // Arrange
            var tile = new SlipperyTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameState();

            // Act
            var events = tile.OnEnter(piece, new Vector2Int(1, 0), state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void SlipperyTile_OnEnter_WithBlockedPath_ReturnsNoEvents()
        {
            // Arrange
            var tile = new SlipperyTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameStateWithMove(new Vector2Int(0, 0), new Vector2Int(1, 0));
            
            // Place a piece in the slide destination
            state.Board.PlacePiece(new TestPiece("Blocking", PlayerColor.Black), new Vector2Int(2, 0));

            // Act
            var events = tile.OnEnter(piece, new Vector2Int(1, 0), state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void SlipperyTile_OnTurnStart_ReturnsNoEvents()
        {
            // Arrange
            var tile = new SlipperyTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            var state = CreateTestGameState();

            // Act
            var events = tile.OnTurnStart(piece, new Vector2Int(0, 0), state).ToList();

            // Assert
            Assert.Empty(events);
        }

        [Fact]
        public void Tiles_Clone_CreatesIndependentCopies()
        {
            // Arrange
            var originalTile = new ScorchedTile { Position = new Vector2Int(1, 1) };

            // Act
            var clonedTile = originalTile.Clone();

            // Assert
            Assert.NotSame(originalTile, clonedTile);
            Assert.Equal(originalTile.Position, clonedTile.Position);
            Assert.IsType<ScorchedTile>(clonedTile);
        }

        [Fact]
        public void Tiles_DoNotMutateBoard_OnlyEmitCandidateEvents()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var scorchedTile = new ScorchedTile();
            var piece = new TestPiece("Test", PlayerColor.White);
            
            // Place piece on board
            board.PlacePiece(piece, new Vector2Int(0, 0));

            // Act - call tile hooks
            var enterEvents = scorchedTile.OnEnter(piece, new Vector2Int(0, 0), state).ToList();
            var turnStartEvents = scorchedTile.OnTurnStart(piece, new Vector2Int(0, 0), state).ToList();

            // Assert - board state should be unchanged
            Assert.NotNull(board.GetPieceAt(new Vector2Int(0, 0)));
            Assert.Equal("Test", board.GetPieceAt(new Vector2Int(0, 0))!.Name);
            
            // But candidate events should be emitted
            Assert.NotEmpty(enterEvents);
            Assert.NotEmpty(turnStartEvents);
        }

        private GameState CreateTestGameState()
        {
            var board = new RogueChess.Engine.Board.Board(8, 8);
            return GameState.CreateInitial(board, PlayerColor.White);
        }

        private GameState CreateTestGameStateWithMove(Vector2Int from, Vector2Int to)
        {
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            // Simulate a move by adding it to the move history
            var move = new Move(from, to, new TestPiece("Test", PlayerColor.White));
            var newState = state.WithUpdatedState(additionalMove: move);
            
            return newState;
        }
    }

}
