using RogueChess.Engine.Board;
using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Tests for Step 7: Hook system with global event interception.
    /// </summary>
    public class HookTests
    {
        [Fact]
        public void HookCollector_CollectHooks_ReturnsHooksFromPieces()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            var rook = new Rook(PlayerColor.White, new Vector2Int(0, 0));
            var martyrRook = new MartyrDecorator(rook);
            board.PlacePiece(martyrRook, new Vector2Int(0, 0));

            // Act
            var hooks = HookCollector.CollectHooks(state).ToList();

            // Assert
            Assert.Single(hooks);
            Assert.IsType<MartyrDecorator>(hooks[0]);
        }

        [Fact]
        public void HookCollector_CollectHooks_ReturnsHooksFromTiles()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            var guardianTile = new GuardianTile { Position = new Vector2Int(3, 3) };
            board.SetTile(new Vector2Int(3, 3), guardianTile);

            // Act
            var hooks = HookCollector.CollectHooks(state).ToList();

            // Assert
            Assert.Single(hooks);
            Assert.IsType<GuardianTile>(hooks[0]);
        }

        [Fact]
        public void HookCollector_CollectHooks_ReturnsHooksFromDecoratedPieces()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            var pawn = new Pawn(PlayerColor.White, new Vector2Int(0, 0));
            var martyrPawn = new MartyrDecorator(pawn);
            var doubleDecorated = new StatusEffectDecorator(martyrPawn);
            board.PlacePiece(doubleDecorated, new Vector2Int(0, 0));

            // Act
            var hooks = HookCollector.CollectHooks(state).ToList();

            // Assert
            Assert.Single(hooks); // Only the MartyrDecorator implements IBeforeEventHook
            Assert.IsType<MartyrDecorator>(hooks[0]);
        }

        [Fact]
        public void MartyrDecorator_BeforeEvent_WithAdjacentFriendlyCapture_RewritesCapture()
        {
            // Arrange
            var martyrRook = new MartyrDecorator(new Rook(PlayerColor.White, new Vector2Int(3, 3)));
            var state = CreateTestGameState();
            
            var friendlyPawn = new Pawn(PlayerColor.White, new Vector2Int(2, 2));
            var captureEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true, // Player action
                new CapturePayload(friendlyPawn)
            );

            // Act
            var result = martyrRook.BeforeEvent(captureEvent, state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GameEventType.PieceCaptured, result.Type);
            Assert.IsType<CapturePayload>(result.Payload);
            var payload = (CapturePayload)result.Payload;
            Assert.Same(martyrRook.Inner, payload.Target); // Should target the inner piece of the martyr
        }

        [Fact]
        public void MartyrDecorator_BeforeEvent_WithNonAdjacentCapture_DoesNotRewrite()
        {
            // Arrange
            var martyrRook = new MartyrDecorator(new Rook(PlayerColor.White, new Vector2Int(3, 3)));
            var state = CreateTestGameState();
            
            var friendlyPawn = new Pawn(PlayerColor.White, new Vector2Int(0, 0)); // Not adjacent
            var captureEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true, // Player action
                new CapturePayload(friendlyPawn)
            );

            // Act
            var result = martyrRook.BeforeEvent(captureEvent, state);

            // Assert
            Assert.Same(captureEvent, result); // Should return original event unchanged
        }

        [Fact]
        public void MartyrDecorator_BeforeEvent_WithEnemyCapture_DoesNotRewrite()
        {
            // Arrange
            var martyrRook = new MartyrDecorator(new Rook(PlayerColor.White, new Vector2Int(3, 3)));
            var state = CreateTestGameState();
            
            var enemyPawn = new Pawn(PlayerColor.Black, new Vector2Int(2, 2)); // Adjacent but enemy
            var captureEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true, // Player action
                new CapturePayload(enemyPawn)
            );

            // Act
            var result = martyrRook.BeforeEvent(captureEvent, state);

            // Assert
            Assert.Same(captureEvent, result); // Should return original event unchanged
        }

        [Fact]
        public void MartyrDecorator_BeforeEvent_WithNonCaptureEvent_DoesNotRewrite()
        {
            // Arrange
            var martyrRook = new MartyrDecorator(new Rook(PlayerColor.White, new Vector2Int(3, 3)));
            var state = CreateTestGameState();
            
            var moveEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true, // Player action
                new MovePayload(new Pawn(PlayerColor.White, new Vector2Int(0, 0)), new Vector2Int(0, 0), new Vector2Int(0, 1))
            );

            // Act
            var result = martyrRook.BeforeEvent(moveEvent, state);

            // Assert
            Assert.Same(moveEvent, result); // Should return original event unchanged
        }

        [Fact]
        public void GuardianTile_BeforeEvent_WithPieceOnTile_CancelsCapture()
        {
            // Arrange
            var guardianTile = new GuardianTile { Position = new Vector2Int(3, 3) };
            var state = CreateTestGameState();
            
            var protectedPiece = new Pawn(PlayerColor.White, new Vector2Int(3, 3));
            var captureEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true, // Player action
                new CapturePayload(protectedPiece)
            );

            // Act
            var result = guardianTile.BeforeEvent(captureEvent, state);

            // Assert
            Assert.Null(result); // Should cancel the capture
        }

        [Fact]
        public void GuardianTile_BeforeEvent_WithPieceNotOnTile_DoesNotCancel()
        {
            // Arrange
            var guardianTile = new GuardianTile { Position = new Vector2Int(3, 3) };
            var state = CreateTestGameState();
            
            var otherPiece = new Pawn(PlayerColor.White, new Vector2Int(0, 0)); // Not on guardian tile
            var captureEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true, // Player action
                new CapturePayload(otherPiece)
            );

            // Act
            var result = guardianTile.BeforeEvent(captureEvent, state);

            // Assert
            Assert.Same(captureEvent, result); // Should return original event unchanged
        }

        [Fact]
        public void HookCollector_CollectHooks_ReturnsHooksInDeterministicOrder()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            
            // Add multiple hooks
            var guardianTile1 = new GuardianTile { Position = new Vector2Int(0, 0) };
            var guardianTile2 = new GuardianTile { Position = new Vector2Int(1, 1) };
            var martyrRook = new MartyrDecorator(new Rook(PlayerColor.White, new Vector2Int(2, 2)));
            
            board.SetTile(new Vector2Int(0, 0), guardianTile1);
            board.SetTile(new Vector2Int(1, 1), guardianTile2);
            board.PlacePiece(martyrRook, new Vector2Int(2, 2));

            // Act
            var hooks1 = HookCollector.CollectHooks(state).ToList();
            var hooks2 = HookCollector.CollectHooks(state).ToList();

            // Assert
            Assert.Equal(hooks1.Count, hooks2.Count);
            for (int i = 0; i < hooks1.Count; i++)
            {
                Assert.Equal(hooks1[i].GetType(), hooks2[i].GetType());
            }
        }

        [Fact]
        public void MartyrDecorator_Clone_PreservesHookBehavior()
        {
            // Arrange
            var originalRook = new Rook(PlayerColor.White, new Vector2Int(3, 3));
            var originalMartyr = new MartyrDecorator(originalRook);

            // Act
            var clonedMartyr = (MartyrDecorator)originalMartyr.Clone();

            // Assert
            Assert.NotSame(originalMartyr, clonedMartyr);
            Assert.IsType<MartyrDecorator>(clonedMartyr);
            Assert.True(clonedMartyr is IBeforeEventHook);
        }

        private GameState CreateTestGameState()
        {
            var board = new RogueChess.Engine.Board.Board(8, 8);
            return GameState.CreateInitial(board, PlayerColor.White);
        }
    }
}
