using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RogueChess.Engine;
using RogueChess.Engine.Board;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.GameModes;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.Tiles;

namespace RogueChess.UI
{
    /// <summary>
    /// Main board form that integrates with the new Engine architecture.
    /// Features undo/redo, debug panel, and canonical event highlighting.
    /// </summary>
    public enum PlayerMode
    {
        HumanVsHuman,
        HumanVsAI,
        AIVsAI,
    }

    public class EngineBoardForm : Form
    {
        private GameEngine? _gameEngine;
        private IRuleSet? _ruleset;

        // Players
        private EngineHumanController? _whiteHuman;
        private EngineHumanController? _blackHuman;
        private IPlayerController? _whitePlayer;
        private IPlayerController? _blackPlayer;
        private IGameMode? _gameMode;
        private PlayerMode _currentPlayerMode = PlayerMode.HumanVsAI;

        private Vector2Int? _lastMoveFrom;
        private Vector2Int? _lastMoveTo;

        private readonly TableLayoutPanel _grid = new()
        {
            Dock = DockStyle.Fill,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        private Button[,]? _cells;

        private Vector2Int? _selected;
        private Vector2Int[] _legalTargets = Array.Empty<Vector2Int>();

        // UI Controls
        private readonly MenuStrip _menuStrip = new();
        private readonly ToolStrip _toolStrip = new();
        private readonly StatusStrip _statusStrip = new();
        private readonly Label _statusLabel = new();

        // Debug panel
        private DebugPanelForm? _debugPanel;

        // Game loop management
        private CancellationTokenSource? _loopCts;
        private Task? _loopTask;

        private EngineHumanController? GetActiveHumanController(PlayerColor color)
        {
            return color == PlayerColor.White ? _whiteHuman : _blackHuman;
        }

        public EngineBoardForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            Text = "Rogue Chess - Engine Demo";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 800); // Larger window for bigger pieces

            // Menu bar
            var gameMenu = new ToolStripMenuItem("Game");
            var newGameItem = new ToolStripMenuItem("New Game", null, NewGame_Click);
            var gameModeMenu = new ToolStripMenuItem("Game Mode");
            var standardModeItem = new ToolStripMenuItem(
                "Standard Chess",
                null,
                (s, e) => SetGameMode(new StandardChessMode())
            );
            var randomModeItem = new ToolStripMenuItem(
                "Random Chess",
                null,
                (s, e) => SetGameMode(new RandomChessMode())
            );

             var draftModeItem = new ToolStripMenuItem(
                "Draft Chess",
                null,
                (s, e) => SetGameMode(new DraftChessMode())
            );

            var playerMenu = new ToolStripMenuItem("Players");
            var humanVsHumanItem = new ToolStripMenuItem(
                "Human vs Human",
                null,
                (s, e) => SetPlayerMode(PlayerMode.HumanVsHuman)
            );
            var humanVsAIItem = new ToolStripMenuItem(
                "Human vs AI",
                null,
                (s, e) => SetPlayerMode(PlayerMode.HumanVsAI)
            );
            var aiVsAIItem = new ToolStripMenuItem(
                "AI vs AI",
                null,
                (s, e) => SetPlayerMode(PlayerMode.AIVsAI)
            );

            var legendItem = new ToolStripMenuItem("Show Legend", null, Legend_Click);
            var debugItem = new ToolStripMenuItem("Show Debug Panel", null, DebugPanel_Click);
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());

            gameModeMenu.DropDownItems.AddRange(
                new ToolStripItem[] { standardModeItem, randomModeItem, draftModeItem }
            );
            playerMenu.DropDownItems.AddRange(
                new ToolStripItem[] { humanVsHumanItem, humanVsAIItem, aiVsAIItem }
            );

            gameMenu.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    newGameItem,
                    new ToolStripSeparator(),
                    gameModeMenu,
                    new ToolStripSeparator(),
                    playerMenu,
                    new ToolStripSeparator(),
                    legendItem,
                    new ToolStripSeparator(),
                    debugItem,
                    new ToolStripSeparator(),
                    exitItem,
                }
            );
            _menuStrip.Items.Add(gameMenu);

            // Toolbar
            var newGameButton = new ToolStripButton("New Game", null, NewGame_Click);
            var undoButton = new ToolStripButton("Undo (Ctrl+Z)", null, Undo_Click)
            {
                Enabled = false,
            };
            var redoButton = new ToolStripButton("Redo (Ctrl+Y)", null, Redo_Click)
            {
                Enabled = false,
            };
            var debugButton = new ToolStripButton("Debug Panel", null, DebugPanel_Click);

            _toolStrip.Items.AddRange(
                new ToolStripItem[]
                {
                    newGameButton,
                    new ToolStripSeparator(),
                    undoButton,
                    redoButton,
                    new ToolStripSeparator(),
                    debugButton,
                }
            );

            // Status bar
            _statusLabel.Text = "Ready";
            _statusStrip.Items.Add(new ToolStripStatusLabel(_statusLabel.Text));

            // Layout
            Controls.Add(_grid);
            Controls.Add(_toolStrip);
            Controls.Add(_menuStrip);
            Controls.Add(_statusStrip);

            // Keyboard shortcuts
            KeyPreview = true;
            KeyDown += EngineBoardForm_KeyDown;

            // Store references for later use
            Tag = new { UndoButton = undoButton, RedoButton = redoButton };
        }

        private void InitializeGame()
        {
            // Set default game mode
            SetGameMode(new StandardChessMode());
        }

        private void SetGameMode(IGameMode gameMode)
        {
            _gameMode = gameMode;
            _ruleset = gameMode.GetRuleSet();

            // Set up players based on current player mode
            SetPlayerControllers();

            // Update window title
            Text = $"Rogue Chess - {gameMode.Name}";

            NewGame();
        }

        private void SetPlayerMode(PlayerMode playerMode)
        {
            _currentPlayerMode = playerMode;
            if (_ruleset != null)
            {
                SetPlayerControllers();
            }
        }

        private void SetPlayerControllers()
        {
            if (_ruleset == null)
                return;

            switch (_currentPlayerMode)
            {
                case PlayerMode.HumanVsHuman:
                    _whitePlayer = _whiteHuman = new EngineHumanController(_ruleset);
                    _blackPlayer = _blackHuman = new EngineHumanController(_ruleset);
                    break;
                case PlayerMode.HumanVsAI:
                    _whitePlayer = _whiteHuman = new EngineHumanController(_ruleset);
                    _blackPlayer = new GreedyAIController(_ruleset);
                    _blackHuman = null;
                    break;
                case PlayerMode.AIVsAI:
                    _whitePlayer = new GreedyAIController(_ruleset);
                    _blackPlayer = new GreedyAIController(_ruleset);
                    _whiteHuman = null;
                    _blackHuman = null;
                    break;
            }
        }

        private void NewGame_Click(object? sender, EventArgs e)
        {
            NewGame();
        }

        private void NewGame()
        {
            // Stop any running game loop
            StopGameLoop();

            if (_gameMode == null)
            {
                SetGameMode(new StandardChessMode());
                return;
            }

            // Create board using the current game mode
            var board = _gameMode.SetupBoard();

            // Create initial game state
            var initialState = GameState.CreateInitial(board, PlayerColor.White);

            // Create game engine
            _gameEngine = new GameEngine(initialState, _whitePlayer!, _blackPlayer!, _ruleset!);

            // Subscribe to events
            _gameEngine.OnEventPublished += OnGameEventPublished;

            // Initialize UI
            InitializeGrid();
            UpdateDisplay();
            UpdateStatus();

            // Start game loop
            StartGameLoop();
        }

        private void InitializeGrid()
        {
            _grid.Controls.Clear();

            // Get board dimensions from current game state
            var boardWidth = 8;
            var boardHeight = 8;
            if (_gameEngine?.CurrentState?.Board != null)
            {
                boardWidth = _gameEngine.CurrentState.Board.Width;
                boardHeight = _gameEngine.CurrentState.Board.Height;
            }

            _grid.RowCount = boardHeight;
            _grid.ColumnCount = boardWidth;
            _cells = new Button[boardWidth, boardHeight];

            for (int y = boardHeight - 1; y >= 0; y--) // Start from top and go down
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    var cell = new Button
                    {
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Arial", 12, FontStyle.Bold), // Smaller font since we're using images
                        Tag = new Vector2Int(x, y),
                        Margin = Padding.Empty,
                        Padding = Padding.Empty,
                    };
                    cell.Click += Cell_Click;
                    _cells[x, y] = cell;
                    _grid.Controls.Add(cell, x, boardHeight - 1 - y); // Reverse y for display
                }
            }

            // Set equal sizing
            for (int i = 0; i < boardHeight; i++)
            {
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / boardHeight));
            }
            for (int i = 0; i < boardWidth; i++)
            {
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / boardWidth));
            }
        }

        private void Cell_Click(object? sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is Vector2Int pos)
            {
                HandleCellClick(pos);
            }
        }

        private void HandleCellClick(Vector2Int pos)
        {
            if (_gameEngine == null)
                return;

            var currentState = _gameEngine.CurrentState;
            var piece = currentState.Board.GetPieceAt(pos);

            // Only allow human interaction when it's the human player's turn
            var controller = GetActiveHumanController(currentState.CurrentPlayer);
            if (controller == null)
                return; // AI’s turn, ignore clicks

            if (_selected == null)
            {
                // First click - select piece
                if (piece != null && piece.Owner == currentState.CurrentPlayer)
                {
                    _selected = pos;
                    _legalTargets = controller
                        .GetLegalMoves(currentState, pos)
                        .Select(m => m.To)
                        .ToArray();
                    UpdateDisplay();
                }
            }
            else
            {
                // Second click - make move
                if (_legalTargets.Contains(pos))
                {
                    var move = new Move(_selected.Value, pos, piece!);
                    controller.SubmitMove(move);

                    // Clear selection after move
                    ClearSelection();
                    UpdateDisplay();
                }
                else
                {
                    // Cancel selection
                    ClearSelection();
                    UpdateDisplay();
                }
            }
        }

        private void ClearSelection()
        {
            _selected = null;
            _legalTargets = Array.Empty<Vector2Int>();
        }

        private void AddPieceVisualEffects(Button cell, IPiece piece)
        {
            // Check for decorators and status effects
            var hasExploding = HasDecorator(piece, typeof(ExplodingDecorator));
            var hasStatusEffect = HasDecorator(piece, typeof(StatusEffectDecorator));
            var hasMartyr = HasDecorator(piece, typeof(MartyrDecorator));

            // Add visual indicators
            if (hasExploding)
            {
                cell.Text = cell.Text + "*";
                cell.ForeColor = Color.Orange;
                cell.Font = new Font("Arial", 8, FontStyle.Bold);
            }

            if (hasStatusEffect)
            {
                // Add a small indicator for status effects
                cell.Text = cell.Text + "●";
                cell.ForeColor = Color.Red;
                cell.Font = new Font("Arial", 8, FontStyle.Bold);
            }

            if (hasMartyr)
            {
                // Add a cross indicator for martyr
                if (string.IsNullOrEmpty(cell.Text))
                {
                    cell.Text = cell.Text + "✚";
                    cell.ForeColor = Color.Blue;
                    cell.Font = new Font("Arial", 8, FontStyle.Bold);
                }
            }
        }

        private void AddTileVisualEffects(Button cell, ITile tile)
        {
            // Add visual indicators for special tiles
            switch (tile)
            {
                case ScorchedTile:
                    // Add a subtle red tint to scorched tiles
                    var baseColor = cell.BackColor;
                    cell.BackColor = Color.FromArgb(
                        Math.Min(255, baseColor.R + 30),
                        Math.Max(0, baseColor.G - 20),
                        Math.Max(0, baseColor.B - 20)
                    );
                    break;

                case SlipperyTile:
                    // Add a subtle blue tint to slippery tiles
                    var baseColor2 = cell.BackColor;
                    cell.BackColor = Color.FromArgb(
                        Math.Max(0, baseColor2.R - 20),
                        Math.Max(0, baseColor2.G - 20),
                        Math.Min(255, baseColor2.B + 30)
                    );
                    break;

                case GuardianTile:
                    // Add a subtle green tint to guardian tiles
                    var baseColor3 = cell.BackColor;
                    cell.BackColor = Color.FromArgb(
                        Math.Max(0, baseColor3.R - 20),
                        Math.Min(255, baseColor3.G + 30),
                        Math.Max(0, baseColor3.B - 20)
                    );
                    break;
            }
        }

        private bool HasDecorator(IPiece piece, Type decoratorType)
        {
            // Check if piece has a specific decorator type
            var current = piece;
            while (current is PieceDecoratorBase decorator)
            {
                if (current.GetType() == decoratorType)
                    return true;
                current = decorator.Inner;
            }
            return false;
        }

        private void UpdateDisplay()
        {
            if (_gameEngine == null || _cells == null)
                return;

            var state = _gameEngine.CurrentState;
            var boardWidth = state.Board.Width;
            var boardHeight = state.Board.Height;

            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var cell = _cells[x, y];
                    var piece = state.Board.GetPieceAt(pos);

                    // Set piece image using SVG
                    if (piece != null)
                    {
                        var color = piece.Owner == PlayerColor.White ? "w" : "b";
                        var name = piece.Name.ToLower();
                        var key = $"{name}-{color}";

                        var size = Math.Min(cell.Width, cell.Height) - 6;
                        var bmp = PieceImageCache.RenderSvg(key, size);

                        cell.Text = "";
                        cell.Image = bmp;
                        cell.ImageAlign = ContentAlignment.MiddleCenter;
                        cell.BackgroundImageLayout = ImageLayout.Zoom;

                        // Add visual indicators for decorators and status effects
                        AddPieceVisualEffects(cell, piece);
                    }
                    else
                    {
                        cell.Text = "";
                        cell.Image = null;
                    }

                    // Set colors and tile effects
                    var isLight = (x + y) % 2 == 0;
                    cell.BackColor = GetCellColor(pos, isLight);
                    cell.FlatAppearance.BorderSize = 0;

                    // Add tile visual effects
                    AddTileVisualEffects(cell, state.Board.GetTile(pos));
                }
            }

            UpdateUndoRedoButtons();
        }

        private Color GetCellColor(Vector2Int pos, bool isLight)
        {
            // Highlight last move
            if (_lastMoveFrom == pos || _lastMoveTo == pos)
            {
                return Color.Yellow;
            }

            // Highlight selected piece
            if (_selected == pos)
            {
                return Color.LightBlue;
            }

            // Highlight legal targets with a more subtle color
            if (_legalTargets.Contains(pos))
            {
                return Color.FromArgb(150, 255, 150); // Semi-transparent green
            }

            // Brown board colors like the original
            return isLight ? Color.FromArgb(240, 217, 181) : Color.FromArgb(181, 136, 99);
        }

        private void UpdateStatus()
        {
            if (_gameEngine == null)
            {
                _statusLabel.Text = "No game";
                return;
            }

            var state = _gameEngine.CurrentState;
            var currentPlayer = state.CurrentPlayer == PlayerColor.White ? "White" : "Black";
            var turnNumber = state.TurnNumber;

            if (_gameEngine.IsGameOver())
            {
                var winner = _gameEngine.GetWinner();
                var winnerText = winner switch
                {
                    PlayerColor.White => "White wins!",
                    PlayerColor.Black => "Black wins!",
                    null => "Draw!",
                    _ => "Game Over",
                };
                _statusLabel.Text = $"{winnerText} (Turn {turnNumber})";
            }
            else
            {
                _statusLabel.Text = $"Turn {turnNumber}: {currentPlayer} to move";
            }
        }

        private void UpdateUndoRedoButtons()
        {
            if (Tag is { } tagObj)
            {
                var undoButton = ((dynamic)tagObj).UndoButton as ToolStripButton;
                var redoButton = ((dynamic)tagObj).RedoButton as ToolStripButton;

                if (_gameEngine != null)
                {
                    undoButton!.Enabled = _gameEngine.CurrentIndex > 0;
                    redoButton!.Enabled = _gameEngine.CurrentIndex < _gameEngine.HistoryCount - 1;
                }
                else
                {
                    undoButton!.Enabled = false;
                    redoButton!.Enabled = false;
                }
            }
        }

        private void StartGameLoop()
        {
            _loopCts = new CancellationTokenSource();
            _loopTask = Task.Run(
                async () =>
                {
                    while (!_loopCts.Token.IsCancellationRequested && _gameEngine != null)
                    {
                        try
                        {
                            if (!_gameEngine.IsGameOver())
                            {
                                _gameEngine.RunTurn();

                                // Update UI on main thread
                                Invoke(
                                    new Action(() =>
                                    {
                                        // Clear selection when game state changes (AI move)
                                        if (
                                            _gameEngine.CurrentState.CurrentPlayer
                                            == PlayerColor.White
                                        )
                                        {
                                            ClearSelection();
                                        }
                                        UpdateDisplay();
                                        UpdateStatus();
                                    })
                                );
                            }

                            await Task.Delay(100, _loopCts.Token); // Small delay to prevent busy waiting
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                },
                _loopCts.Token
            );
        }

        private void StopGameLoop()
        {
            _loopCts?.Cancel();
            _loopTask?.Wait(1000); // Wait up to 1 second for graceful shutdown
            _loopCts?.Dispose();
            _loopCts = null;
            _loopTask = null;
        }

        private void OnGameEventPublished(GameEvent gameEvent)
        {
            // Update debug panel if it's open
            _debugPanel?.AddEvent(gameEvent, _gameEngine.CurrentState);

            // Update last move highlighting for player actions
            if (gameEvent.IsPlayerAction && gameEvent.Payload is MovePayload movePayload)
            {
                _lastMoveFrom = movePayload.From;
                _lastMoveTo = movePayload.To;
            }
        }

        private void Undo_Click(object? sender, EventArgs e)
        {
            if (_gameEngine != null && _gameEngine.CurrentIndex > 0)
            {
                _gameEngine.UndoLastMove();
                _selected = null;
                _legalTargets = Array.Empty<Vector2Int>();
                UpdateDisplay();
                UpdateStatus();
            }
        }

        private void Redo_Click(object? sender, EventArgs e)
        {
            if (_gameEngine != null && _gameEngine.CurrentIndex < _gameEngine.HistoryCount - 1)
            {
                _gameEngine.RedoLastMove();
                _selected = null;
                _legalTargets = Array.Empty<Vector2Int>();
                UpdateDisplay();
                UpdateStatus();
            }
        }

        private void DebugPanel_Click(object? sender, EventArgs e)
        {
            if (_debugPanel == null || _debugPanel.IsDisposed)
            {
                _debugPanel = new DebugPanelForm();
                _debugPanel.Show();
            }
            else
            {
                _debugPanel.BringToFront();
            }
        }

        private void Legend_Click(object? sender, EventArgs e)
        {
            var legendForm = new LegendForm();
            legendForm.ShowDialog();
        }

        private void EngineBoardForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z:
                        Undo_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Keys.Y:
                        Redo_Click(sender, e);
                        e.Handled = true;
                        break;
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopGameLoop();
            _whiteHuman?.CancelPending();
            _blackHuman?.CancelPending();
            _debugPanel?.Close();
            base.OnFormClosed(e);
        }
    }
}
