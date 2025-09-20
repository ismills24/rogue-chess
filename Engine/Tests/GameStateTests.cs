using RogueChess.Engine.Board;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Basic tests for Step 3: GameState deep cloning.
    /// This verifies that changes to a cloned GameState don't affect the original.
    /// </summary>
    public class GameStateTests
    {
        [Fact]
        public void Clone_IsDeepCopy_ChangesToCloneDoNotAffectOriginal()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var originalState = GameState.CreateInitial(board, PlayerColor.White);

            // Act
            var clonedState = originalState.Clone();

            // Modify the cloned board
            var clonedBoard = (RogueChess.Engine.Board.Board)clonedState.Board;
            clonedBoard.PlacePiece(new TestPiece("Test", PlayerColor.White), new Vector2Int(0, 0));

            // Assert
            Assert.Null(originalState.Board.GetPieceAt(new Vector2Int(0, 0)));
            Assert.NotNull(clonedState.Board.GetPieceAt(new Vector2Int(0, 0)));
        }

        [Fact]
        public void Clone_PreservesAllProperties()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var originalState = GameState.CreateInitial(board, PlayerColor.Black);

            // Act
            var clonedState = originalState.Clone();

            // Assert
            Assert.Equal(originalState.CurrentPlayer, clonedState.CurrentPlayer);
            Assert.Equal(originalState.TurnNumber, clonedState.TurnNumber);
            Assert.Equal(originalState.MoveHistory.Count, clonedState.MoveHistory.Count);
        }

        [Fact]
        public void CreateInitial_SetsCorrectInitialValues()
        {
            // Arrange & Act
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);

            // Assert
            Assert.Equal(PlayerColor.White, state.CurrentPlayer);
            Assert.Equal(1, state.TurnNumber);
            Assert.Empty(state.MoveHistory);
            Assert.Same(board, state.Board);
        }
    }

    /// <summary>
    /// Simple test piece implementation for testing.
    /// </summary>
    public class TestPiece : IPiece
    {
        public string Name { get; }
        public PlayerColor Owner { get; }
        public Vector2Int Position { get; set; }

        public TestPiece(string name, PlayerColor owner)
        {
            Name = name;
            Owner = owner;
        }

        public IEnumerable<Move> GetPseudoLegalMoves(GameState state) => Enumerable.Empty<Move>();

        public IEnumerable<CandidateEvent> OnMove(Move move, GameState state) =>
            Enumerable.Empty<CandidateEvent>();

        public IEnumerable<CandidateEvent> OnCapture(GameState state) =>
            Enumerable.Empty<CandidateEvent>();

        public int GetValue() => 1;

        public IPiece Clone() => new TestPiece(Name, Owner) { Position = Position };
    }
}
