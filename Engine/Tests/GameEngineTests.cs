using RogueChess.Engine.Board;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using Xunit;

namespace RogueChess.Engine.Tests
{
    /// <summary>
    /// Tests for Step 8: GameEngine with canonical pipeline.
    /// </summary>
    public class GameEngineTests
    {
        [Fact]
        public void GameEngine_Constructor_InitializesWithInitialState()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);

            // Act
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Assert
            Assert.Equal(PlayerColor.White, engine.CurrentState.CurrentPlayer);
            Assert.Equal(1, engine.CurrentState.TurnNumber);
            Assert.Equal(0, engine.CurrentIndex);
        }

        [Fact]
        public void GameEngine_RunTurn_WithNoPieces_DoesNotAdvance()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            var initialIndex = engine.CurrentIndex;

            // Act
            engine.RunTurn();

            // Assert
            Assert.Equal(initialIndex, engine.CurrentIndex); // Should not advance
        }

        [Fact]
        public void GameEngine_RunTurn_WithPieces_AdvancesTurn()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add some pieces
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackKing = new King(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackKing, new Vector2Int(7, 7));

            var initialIndex = engine.CurrentIndex;

            // Act
            engine.RunTurn();

            // Assert
            Assert.True(engine.CurrentIndex > initialIndex); // Should advance
        }

        [Fact]
        public void GameEngine_Commit_WithNoHooks_CreatesCanonicalEvent()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            var candidateEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true,
                new MovePayload(new King(PlayerColor.White, new Vector2Int(0, 0)), new Vector2Int(0, 0), new Vector2Int(0, 1))
            );

            // Act
            var canonicalEvent = engine.Commit(candidateEvent);

            // Assert
            Assert.NotNull(canonicalEvent);
            Assert.Equal(GameEventType.MoveApplied, canonicalEvent.Type);
            Assert.True(canonicalEvent.IsPlayerAction);
            Assert.NotEqual(Guid.Empty, canonicalEvent.Id);
        }

        [Fact]
        public void GameEngine_Commit_WithHooks_ProcessesThroughHooks()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add a martyr piece
            var rook = new Rook(PlayerColor.White, new Vector2Int(0, 0));
            var martyrRook = new MartyrDecorator(rook);
            board.PlacePiece(martyrRook, new Vector2Int(0, 0));

            var friendlyPawn = new Pawn(PlayerColor.White, new Vector2Int(1, 1));
            board.PlacePiece(friendlyPawn, new Vector2Int(1, 1));

            var candidateEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true,
                new CapturePayload(friendlyPawn)
            );

            // Act
            var canonicalEvent = engine.Commit(candidateEvent);

            // Assert
            Assert.NotNull(canonicalEvent);
            Assert.Equal(GameEventType.PieceCaptured, canonicalEvent.Type);
            // The martyr should have rewritten the capture to target itself
            Assert.IsType<CapturePayload>(canonicalEvent.Payload);
        }

        [Fact]
        public void GameEngine_Undo_GoesBackInHistory()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add pieces and run a turn
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackKing = new King(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackKing, new Vector2Int(7, 7));

            engine.RunTurn();
            var afterTurnIndex = engine.CurrentIndex;

            // Act
            engine.Undo();

            // Assert
            Assert.True(engine.CurrentIndex < afterTurnIndex);
        }

        [Fact]
        public void GameEngine_Redo_GoesForwardInHistory()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add pieces and run a turn
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackKing = new King(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackKing, new Vector2Int(7, 7));

            engine.RunTurn();
            var afterTurnIndex = engine.CurrentIndex;
            engine.Undo();

            // Act
            engine.Redo();

            // Assert
            Assert.Equal(afterTurnIndex, engine.CurrentIndex);
        }

        [Fact]
        public void GameEngine_JumpTo_GoesToSpecificIndex()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add pieces and run a few turns
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            var blackKing = new King(PlayerColor.Black, new Vector2Int(7, 7));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));
            board.PlacePiece(blackKing, new Vector2Int(7, 7));

            engine.RunTurn();
            engine.RunTurn();
            var targetIndex = 1;

            // Act
            engine.JumpTo(targetIndex);

            // Assert
            Assert.Equal(targetIndex, engine.CurrentIndex);
        }

        [Fact]
        public void GameEngine_IsGameOver_WithNoPieces_ReturnsTrue()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Act
            var isGameOver = engine.IsGameOver();

            // Assert
            Assert.True(isGameOver);
        }

        [Fact]
        public void GameEngine_GetWinner_WithOnlyWhitePieces_ReturnsWhite()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add only white pieces
            var whiteKing = new King(PlayerColor.White, new Vector2Int(0, 0));
            board.PlacePiece(whiteKing, new Vector2Int(0, 0));

            // Act
            var winner = engine.GetWinner();

            // Assert
            Assert.Equal(PlayerColor.White, winner);
        }

        [Fact]
        public void GameEngine_OnEventPublished_IsTriggeredOnCommit()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            var publishedEvents = new List<GameEvent>();
            engine.OnEventPublished += publishedEvents.Add;

            var candidateEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true,
                new MovePayload(new King(PlayerColor.White, new Vector2Int(0, 0)), new Vector2Int(0, 0), new Vector2Int(0, 1))
            );

            // Act
            engine.Commit(candidateEvent);

            // Assert
            Assert.Single(publishedEvents);
            Assert.Equal(GameEventType.MoveApplied, publishedEvents[0].Type);
        }

        [Fact]
        public void GameEngine_Commit_WithCancelledEvent_ReturnsNull()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Add a guardian tile that cancels captures
            var guardianTile = new GuardianTile { Position = new Vector2Int(0, 0) };
            board.SetTile(new Vector2Int(0, 0), guardianTile);

            var piece = new Pawn(PlayerColor.White, new Vector2Int(0, 0));
            board.PlacePiece(piece, new Vector2Int(0, 0));

            var candidateEvent = new CandidateEvent(
                GameEventType.PieceCaptured,
                true,
                new CapturePayload(piece)
            );

            // Act
            var result = engine.Commit(candidateEvent);

            // Assert
            Assert.Null(result); // Should be cancelled by guardian tile
        }
    }
}
