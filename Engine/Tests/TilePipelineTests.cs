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
    /// Tests for Step 9: Tile Examples via Pipeline.
    /// Verifies that ScorchedTile and SlipperyTile work through the canonical pipeline.
    /// </summary>
    public class TilePipelineTests
    {
        [Fact]
        public void ScorchedTile_OnEnter_EmitsStatusApplyCandidate()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var scorchedTile = new ScorchedTile { Position = new Vector2Int(3, 3) };
            var piece = new Pawn(PlayerColor.White, new Vector2Int(3, 3));

            // Act
            var events = scorchedTile.OnEnter(piece, new Vector2Int(3, 3), state).ToList();

            // Assert
            Assert.Single(events);
            var candidateEvent = events[0];
            Assert.Equal(GameEventType.StatusEffectTriggered, candidateEvent.Type);
            Assert.False(candidateEvent.IsPlayerAction);
            Assert.IsType<StatusApplyPayload>(candidateEvent.Payload);
            
            var payload = (StatusApplyPayload)candidateEvent.Payload;
            Assert.Same(piece, payload.Target);
            Assert.IsType<StatusEffects.BurningStatus>(payload.Effect);
        }

        [Fact]
        public void SlipperyTile_OnEnter_EmitsForcedSlideCandidate()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var state = GameState.CreateInitial(board, PlayerColor.White);
            var slipperyTile = new SlipperyTile { Position = new Vector2Int(3, 3) };
            var piece = new Pawn(PlayerColor.White, new Vector2Int(3, 3));

            // Create a move history to simulate piece landing on slippery tile
            var moveHistory = new List<Move> { new Move(new Vector2Int(2, 3), new Vector2Int(3, 3), piece) };
            var stateWithHistory = new GameState(board, PlayerColor.White, 1).WithUpdatedState(additionalMove: moveHistory[0]);

            // Act
            var events = slipperyTile.OnEnter(piece, new Vector2Int(3, 3), stateWithHistory).ToList();

            // Assert
            Assert.Single(events);
            var candidateEvent = events[0];
            Assert.Equal(GameEventType.TileEffectTriggered, candidateEvent.Type);
            Assert.False(candidateEvent.IsPlayerAction);
            Assert.IsType<ForcedSlidePayload>(candidateEvent.Payload);
            
            var payload = (ForcedSlidePayload)candidateEvent.Payload;
            Assert.Same(piece, payload.Piece);
            Assert.Equal(new Vector2Int(3, 3), payload.From);
            Assert.Equal(new Vector2Int(4, 3), payload.To); // Should slide one more step right
        }

        [Fact]
        public void GameEngine_Commit_WithStatusApplyPayload_AppliesStatusEffect()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            var piece = new Pawn(PlayerColor.White, new Vector2Int(3, 3));
            board.PlacePiece(piece, new Vector2Int(3, 3));

            var candidateEvent = new CandidateEvent(
                GameEventType.StatusEffectTriggered,
                false, // Not a player action
                new StatusApplyPayload(piece, new StatusEffects.BurningStatus())
            );

            // Act
            var canonicalEvent = engine.Commit(candidateEvent);

            // Assert
            Assert.NotNull(canonicalEvent);
            Assert.Equal(GameEventType.StatusEffectTriggered, canonicalEvent.Type);
            
            // Check that the piece is now wrapped with StatusEffectDecorator
            var pieceAtPosition = engine.CurrentState.Board.GetPieceAt(new Vector2Int(3, 3));
            Assert.NotNull(pieceAtPosition);
            Assert.IsType<StatusEffectDecorator>(pieceAtPosition);
        }

        [Fact]
        public void GameEngine_Commit_WithForcedSlidePayload_MovesPiece()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            var piece = new Pawn(PlayerColor.White, new Vector2Int(3, 3));
            board.PlacePiece(piece, new Vector2Int(3, 3));

            var candidateEvent = new CandidateEvent(
                GameEventType.TileEffectTriggered,
                false, // Not a player action
                new ForcedSlidePayload(piece, new Vector2Int(3, 3), new Vector2Int(4, 3))
            );

            // Act
            var canonicalEvent = engine.Commit(candidateEvent);

            // Assert
            Assert.NotNull(canonicalEvent);
            Assert.Equal(GameEventType.TileEffectTriggered, canonicalEvent.Type);
            
            // Check that the piece moved to the new position
            Assert.Null(engine.CurrentState.Board.GetPieceAt(new Vector2Int(3, 3)));
            Assert.NotNull(engine.CurrentState.Board.GetPieceAt(new Vector2Int(4, 3)));
            Assert.Equal(new Vector2Int(4, 3), piece.Position);
        }

        [Fact]
        public void ScorchedTile_ThroughPipeline_ProducesCanonicalStatusEvent()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Set up scorched tile
            var scorchedTile = new ScorchedTile { Position = new Vector2Int(3, 3) };
            board.SetTile(new Vector2Int(3, 3), scorchedTile);

            var piece = new Pawn(PlayerColor.White, new Vector2Int(2, 3));
            board.PlacePiece(piece, new Vector2Int(2, 3));

            var publishedEvents = new List<GameEvent>();
            engine.OnEventPublished += publishedEvents.Add;

            // Act - Move piece onto scorched tile
            var moveEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true, // Player action
                new MovePayload(piece, new Vector2Int(2, 3), new Vector2Int(3, 3))
            );
            engine.Commit(moveEvent);

            // Get the candidate event from the tile
            var tileEvents = scorchedTile.OnEnter(piece, new Vector2Int(3, 3), engine.CurrentState).ToList();
            if (tileEvents.Any())
            {
                engine.Commit(tileEvents[0]);
            }

            // Assert
            Assert.Contains(publishedEvents, e => e.Type == GameEventType.StatusEffectTriggered);
            var statusEvent = publishedEvents.First(e => e.Type == GameEventType.StatusEffectTriggered);
            Assert.IsType<StatusApplyPayload>(statusEvent.Payload);
        }

        [Fact]
        public void SlipperyTile_ThroughPipeline_ProducesCanonicalSlideEvent()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Set up slippery tile
            var slipperyTile = new SlipperyTile { Position = new Vector2Int(3, 3) };
            board.SetTile(new Vector2Int(3, 3), slipperyTile);

            var piece = new Pawn(PlayerColor.White, new Vector2Int(2, 3));
            board.PlacePiece(piece, new Vector2Int(2, 3));

            var publishedEvents = new List<GameEvent>();
            engine.OnEventPublished += publishedEvents.Add;

            // Act - Move piece onto slippery tile
            var moveEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true, // Player action
                new MovePayload(piece, new Vector2Int(2, 3), new Vector2Int(3, 3))
            );
            engine.Commit(moveEvent);

            // Create a move history for the slippery tile
            var moveHistory = new List<Move> { new Move(new Vector2Int(2, 3), new Vector2Int(3, 3), piece) };
            var stateWithHistory = new GameState(engine.CurrentState.Board, engine.CurrentState.CurrentPlayer, engine.CurrentState.TurnNumber).WithUpdatedState(additionalMove: moveHistory[0]);

            // Get the candidate event from the tile
            var tileEvents = slipperyTile.OnEnter(piece, new Vector2Int(3, 3), stateWithHistory).ToList();
            if (tileEvents.Any())
            {
                engine.Commit(tileEvents[0]);
            }

            // Assert
            Assert.Contains(publishedEvents, e => e.Type == GameEventType.TileEffectTriggered);
            var slideEvent = publishedEvents.First(e => e.Type == GameEventType.TileEffectTriggered);
            Assert.IsType<ForcedSlidePayload>(slideEvent.Payload);
        }

        [Fact]
        public void TileEffects_WithMultiplePieces_OnlyAffectCorrectPieces()
        {
            // Arrange
            var board = new RogueChess.Engine.Board.Board(8, 8);
            var initialState = GameState.CreateInitial(board, PlayerColor.White);
            var ruleset = new LastPieceStandingRuleSet();
            var whiteController = new RandomAIController(ruleset);
            var blackController = new RandomAIController(ruleset);
            var engine = new GameEngine(initialState, whiteController, blackController, ruleset);

            // Set up scorched tile
            var scorchedTile = new ScorchedTile { Position = new Vector2Int(3, 3) };
            board.SetTile(new Vector2Int(3, 3), scorchedTile);

            var piece1 = new Pawn(PlayerColor.White, new Vector2Int(2, 3));
            var piece2 = new Pawn(PlayerColor.Black, new Vector2Int(4, 3));
            board.PlacePiece(piece1, new Vector2Int(2, 3));
            board.PlacePiece(piece2, new Vector2Int(4, 3));

            var publishedEvents = new List<GameEvent>();
            engine.OnEventPublished += publishedEvents.Add;

            // Act - Move piece1 onto scorched tile
            var moveEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true, // Player action
                new MovePayload(piece1, new Vector2Int(2, 3), new Vector2Int(3, 3))
            );
            engine.Commit(moveEvent);

            // Get the candidate event from the tile
            var tileEvents = scorchedTile.OnEnter(piece1, new Vector2Int(3, 3), engine.CurrentState).ToList();
            if (tileEvents.Any())
            {
                engine.Commit(tileEvents[0]);
            }

            // Assert
            var statusEvents = publishedEvents.Where(e => e.Type == GameEventType.StatusEffectTriggered).ToList();
            Assert.Single(statusEvents);
            
            var statusPayload = (StatusApplyPayload?)statusEvents[0].Payload;
            Assert.NotNull(statusPayload);
            Assert.Same(piece1, statusPayload.Target); // Should only affect piece1, not piece2
        }
    }
}
