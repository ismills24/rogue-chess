# Rogue Chess — Core API

A modular C# framework for experimenting with chess-like roguelike mechanics.
This library is engine-agnostic (usable outside Unity) and provides the simulation backend for the Rogue Chess project.

## ✨ High-Level Concepts

- **Pieces:** Implement `IPiece`. Responsible for exposing pseudo-legal moves and responding to move/capture hooks. Can be decorated to gain new abilities or carry status effects.
- **Boards & Tiles:** Implement `IBoard` and `ITile`. Boards hold piece and tile state; tiles provide entry/turn effects (e.g. slippery, scorched).
- **Rulesets:** Implement `IRuleSet`. Define how pseudo-legal moves are filtered into legal moves, and determine win conditions (checkmate, survival, etc.).
- **GameState:** Immutable-ish snapshot of the current board, player turn, move history. Provides cloning, undo, and move application.
- **GameRunner:** Orchestrates turns, enforces rulesets, publishes events.
- **Events:** All state changes emit `GameEvent`s, allowing UIs or animations to subscribe without being entangled in the core logic.
- **Status Effects & Decorators:** Extend pieces with runtime abilities (burning, exploding, etc.) without rewriting base classes.

## 📂 Folder Structure

```
Core/
 ├── Board/
 │    ├── IBoard.cs           # Board contract
 │    ├── BoardBase.cs        # Default implementation
 │    ├── StandardBoard.cs    # 8x8 chess board
 │    └── Tiles/
 │         ├── ITile.cs
 │         ├── StandardTile.cs
 │         ├── ScorchedTile.cs
 │         └── SlipperyTile.cs
 │
 ├── Pieces/
 │    ├── IPiece.cs           # Base interface
 │    ├── Helpers/Movement.cs # Sliding, jumping, pawn moves, etc.
 │    ├── King.cs …           # All standard chess pieces
 │    └── Decorators/
 │         ├── ExplodingDecorator.cs
 │         └── StatusEffectDecorator.cs
 │
 ├── RuleSets/
 │    ├── IRuleSet.cs
 │    ├── StandardChessRuleSet.cs
 │    └── … (boss-fight rulesets here)
 │
 ├── WinConditions/
 │    ├── IWinCondition.cs
 │    └── CheckmateCondition.cs
 │
 ├── StatusEffects/
 │    ├── IStatusEffect.cs
 │    ├── IStatusEffectCarrier.cs
 │    └── BurningStatus.cs
 │
 ├── Events/
 │    ├── GameEvent.cs
 │    ├── GameEventType.cs
 │
 ├── Runner/
 │    ├── GameRunner.cs
 │    ├── IPlayerController.cs
 │    ├── HumanController.cs
 │    └── RandomAIController.cs
 │
 ├── GameState.cs
 └── Move.cs
```

## 🔑 Core Interfaces

### Pieces

```csharp
public interface IPiece
{
	PlayerColor Owner { get; }
	Vector2Int Position { get; set; }
	string Name { get; }

	IEnumerable<Move> GetPseudoLegalMoves(GameState state);
	void OnMove(Move move, GameState state);
	void OnCapture(GameState state);
	IPiece Clone();
}
```

Use helpers from `Movement.cs` to implement moves.  
Pieces should only produce pseudo-legal moves; legality is determined by the ruleset.

### Boards & Tiles

```csharp
public interface IBoard
{
	int Width { get; }
	int Height { get; }

	IPiece GetPieceAt(Vector2Int pos);
	void PlacePiece(IPiece piece, Vector2Int pos);
	void RemovePiece(Vector2Int pos);
	void MovePiece(Vector2Int from, Vector2Int to);
	IEnumerable<IPiece> GetAllPieces(PlayerColor color);

	bool IsInBounds(Vector2Int pos);
	ITile GetTile(Vector2Int pos);
	void SetTile(Vector2Int pos, ITile tile);

	IBoard Clone();
}

public interface ITile
{
	bool CanEnter(IPiece piece, Vector2Int pos, GameState state);
	IEnumerable<GameEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state);
	IEnumerable<GameEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state);
}
```

**Examples:**

- `StandardTile` — does nothing special.
- `ScorchedTile` — applies a `BurningStatus`.
- `SlipperyTile` — auto-slides pieces one extra step.

### Rulesets

```csharp
public interface IRuleSet
{
	IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece);
	bool IsGameOver(GameState state, out PlayerColor winner);
}
```

- `StandardChessRuleSet` filters moves to disallow self-check and uses `CheckmateCondition`.
- Custom rulesets can ignore check, require survival until X turns, or enforce boss-specific victory conditions.

### Status Effects

```csharp
public interface IStatusEffect
{
	string Name { get; }
	int Duration { get; }

	IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state);
	IEnumerable<GameEvent> OnRemove(IPiece piece, GameState state);

	IStatusEffect Clone();
}
```

- `BurningStatus` — ticks down, destroys piece after N turns.
- Pieces support effects by being wrapped in a `StatusEffectDecorator`.

### Events

All actions emit `GameEvent`s:

```csharp
public enum GameEventType
{
	MoveApplied,
	PieceCaptured,
	PiecePromoted,
	TileEffectTriggered,
	StatusEffectTriggered,
	TurnAdvanced,
	GameOver
}

public class GameEvent
{
	public GameEventType Type { get; }
	public IPiece? Piece { get; }
	public Vector2Int? From { get; }
	public Vector2Int? To { get; }
	public string Message { get; }
}
```

Consumers (UI, animation, logging) subscribe to `GameRunner.OnEventPublished`.

### Game Runner

```csharp
public class GameRunner
{
	public event Action<GameEvent>? OnEventPublished;

	public GameRunner(GameState state, IPlayerController white, IPlayerController black, IRuleSet ruleset);

	public void RunTurn();   // Advance one full turn
	public GameState GetState();
}
```

Handles:

- Turn order
- Applying moves
- Checking ruleset win conditions
- Publishing all events (moves, captures, tile/status triggers, game over)

### Controllers

```csharp
public interface IPlayerController
{
	Move? SelectMove(GameState state);
}
```

- `HumanController` — CLI input (e2 e4).
- `RandomAIController` — picks random legal move.
- Future: AI controller for enemy bosses.

## 🛠️ Example: Running a Standard Chess Demo

```csharp
var ruleSet = new StandardChessRuleSet();
var board = new StandardBoard(8, 8);

// Place pieces (see Program.cs for full setup)
board.PlacePiece(new King(PlayerColor.White, new Vector2Int(4, 0)), new Vector2Int(4, 0));
board.PlacePiece(new King(PlayerColor.Black, new Vector2Int(4, 7)), new Vector2Int(4, 7));

var state = new GameState(board, PlayerColor.White);

var runner = new GameRunner(
	state,
	new HumanController(ruleSet), 
	new RandomAIController(ruleSet),
	ruleSet
);

runner.OnEventPublished += ev => Console.WriteLine($"EVENT: {ev.Message}");

// Run loop
while (true)
	runner.RunTurn();
