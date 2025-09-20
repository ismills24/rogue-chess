# Rogue Chess — Core Engine

A modular C# framework for experimenting with chess-like roguelike mechanics.
This library is engine-agnostic (usable outside Unity) and provides the full simulation backend for the Rogue Chess project.

## ✨ Core Concepts

**Pieces:** Implement `IPiece`. Pieces expose pseudo-legal moves and can be decorated with abilities (exploding, martyr, etc.) or carry status effects (burning, poisoned).

**Boards & Tiles:** Implement `IBoard` and `ITile`. Boards store piece/tile state; tiles apply effects like sliding, burning, or protecting.

**Rulesets:** Implement `IRuleSet`. Decide how pseudo-legal moves become legal moves and determine win conditions.

**GameState:** Immutable snapshot of the board + turn. Supports cloning, simulation, undo/redo, and evaluation.

**GameEngine:** Central orchestrator. Runs turns, applies moves through a candidate→hooks→commit pipeline, and maintains canonical history.

**Events:** All state changes emit `GameEvent`s, which UIs or AI can subscribe to.

**Decorators & Status Effects:** Extend pieces dynamically at runtime with modular abilities.

**Controllers (AI/Human):** Decide moves for each side. Includes `RandomAIController` and `MinimaxAIController` (depth search).

## �� Folder Structure

```
Engine/
 ├── Board/
 │    ├── IBoard.cs
 │    ├── Board.cs
 │    └── Tiles/
 │         ├── StandardTile.cs
 │         ├── ScorchedTile.cs
 │         ├── SlipperyTile.cs
 │         └── GuardianTile.cs
 │
 ├── Pieces/
 │    ├── King.cs … Pawn.cs
 │    ├── Decorators/
 │    │     ├── ExplodingDecorator.cs
 │    │     ├── MartyrDecorator.cs
 │    │     └── StatusEffectDecorator.cs
 │    └── PieceHelpers.cs  # e.g. PieceValueCalculator
 │
 ├── StatusEffects/
 │    ├── IStatusEffect.cs
 │    ├── BurningStatus.cs
 │    └── … (more effects)
 │
 ├── RuleSets/
 │    ├── IRuleSet.cs
 │    ├── StandardChessRuleSet.cs
 │    ├── LastPieceStandingRuleSet.cs
 │    └── … (custom)
 │
 ├── Controllers/
 │    ├── IPlayerController.cs
 │    ├── HumanController.cs
 │    ├── RandomAIController.cs
 │    └── MinimaxAIController.cs
 │
 ├── Engine/
 │    ├── GameEngine.cs      # canonical state + history
 │    ├── ProcessMove.cs     # move application pipeline
 │    ├── Turns.cs           # turn start/end ticks
 │    ├── Events.cs          # GameEvent, CandidateEvent, payloads
 │    └── Simulation.cs      # simulate turns for AI
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
    IEnumerable<CandidateEvent> OnMove(Move move, GameState state);
    IEnumerable<CandidateEvent> OnCapture(GameState state);

    IPiece Clone();
    int GetValue();  // base value for evaluation
}
```

Pieces output pseudo-legal moves. Legality is enforced by the ruleset.

### Boards & Tiles

```csharp
public interface IBoard
{
    int Width { get; }
    int Height { get; }

    IPiece? GetPieceAt(Vector2Int pos);
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
    IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state);
    IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state);
}
```

### Rulesets

```csharp
public interface IRuleSet
{
    IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece);
    bool IsGameOver(GameState state, out PlayerColor winner);
}
```

- Standard chess ruleset (check, checkmate).
- Roguelike rulesets (last piece standing, survival waves).

### Status Effects

```csharp
public interface IStatusEffect
{
    string Name { get; }
    IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, GameState state);
    int ValueModifier();
    IStatusEffect Clone();
}
```

**Example:**

`BurningStatus`: ticks down, eventually destroys the piece.

### Events

```csharp
public enum GameEventType
{
    MoveApplied,
    MoveCancelled,
    PieceCaptured,
    PieceDestroyed,
    PiecePromoted,
    TileEffectTriggered,
    StatusEffectTriggered,
    StatusTick,
    TurnAdvanced,
    GameOver
}
```

All state changes flow through a candidate → hooks → canonical pipeline.
Hooks (e.g., `IBeforeEventHook`) can cancel or replace events.

### Game Engine

```csharp
public partial class GameEngine
{
    public GameState CurrentState { get; }
    public event Action<GameEvent>? OnEventPublished;

    public void RunTurn();        // full cycle (turn start → move → end → advance)
    public void ProcessMove(Move move);
    public void UndoLastMove();   // undo to last player action
}
```

Split into partials:

- `ProcessMove.cs`: captures, moves, tiles, piece effects
- `Turns.cs`: start/end turn ticks
- `Events.cs`: `CandidateEvent` → `GameEvent` pipeline
- `Simulation.cs`: simulate turns for AI search

### Controllers

```csharp
public interface IPlayerController
{
    Move? SelectMove(GameState state);
}
```

- `HumanController`: user input
- `RandomAIController`: random legal move
- `MinimaxAIController`: depth-N search with evaluation via `PieceValueCalculator`

## 🛠 Example

```csharp
var rules = new StandardChessRuleSet();
var board = new StandardBoard(8, 8);

// Kings
board.PlacePiece(new King(PlayerColor.White, new Vector2Int(4,0)), new Vector2Int(4,0));
board.PlacePiece(new King(PlayerColor.Black, new Vector2Int(4,7)), new Vector2Int(4,7));

var state = GameState.CreateInitial(board, PlayerColor.White);

var engine = new GameEngine(
    state,
    new MinimaxAIController(rules, depth: 2),
    new RandomAIController(rules),
    rules
);

engine.OnEventPublished += ev => Console.WriteLine($"{ev.Type}: {ev.Payload}");

while (!engine.IsGameOver())
    engine.RunTurn();
```
```

The README.md has been updated with the new content, properly formatted in markdown. The new version reflects the updated architecture with:

- Split GameEngine partials (ProcessMove.cs, Turns.cs, Events.cs, Simulation.cs)
- Events/hooks system with CandidateEvent pipeline
- MinimaxAIController for AI depth search
- Roguelike mechanics and status effects
- Updated folder structure matching the current codebase
- More detailed interface documentation
- Better organization of core concepts

The markdown formatting includes proper code blocks, headers, lists, and emphasis to make the documentation clear and readable.
