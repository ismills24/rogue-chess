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
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.UI
{
    /// <summary>
    /// Main board form that integrates with the new Engine architecture.
    /// Features undo/redo, debug panel, and canonical event highlighting.
    /// </summary>
    public class EngineBoardForm : Form
    {
        private GameEngine? _gameEngine;
        private IRuleSet? _ruleset;

        // Players
        private EngineHumanController? _whiteHuman;
        private EngineHumanController? _blackHuman;
        private IPlayerController? _whitePlayer;
        private IPlayerController? _blackPlayer;

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
            var debugItem = new ToolStripMenuItem("Show Debug Panel", null, DebugPanel_Click);
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());

            gameMenu.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    newGameItem,
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
            // Create ruleset
            _ruleset = new StandardChessRuleSet();

            // Create controllers - Human vs AI
            _whiteHuman = new EngineHumanController(_ruleset);
            _blackHuman = new EngineHumanController(_ruleset);

            // White is human, Black is AI
            _whitePlayer = _whiteHuman;
            _blackPlayer = new RandomAIController(_ruleset);

            NewGame();
        }

        private void NewGame_Click(object? sender, EventArgs e)
        {
            NewGame();
        }

        private void NewGame()
        {
            // Stop any running game loop
            StopGameLoop();

            // Create new board
            var board = new RogueChess.Engine.Board.Board(8, 8);

            // Set up initial pieces (simplified setup for demo)
            SetupInitialPieces(board);

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

        private void SetupInitialPieces(RogueChess.Engine.Board.Board board)
        {
            // White pieces
            board.PlacePiece(
                new Rook(PlayerColor.White, new Vector2Int(0, 0)),
                new Vector2Int(0, 0)
            );
            board.PlacePiece(
                new Knight(PlayerColor.White, new Vector2Int(1, 0)),
                new Vector2Int(1, 0)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.White, new Vector2Int(2, 0)),
                new Vector2Int(2, 0)
            );
            board.PlacePiece(
                new Queen(PlayerColor.White, new Vector2Int(3, 0)),
                new Vector2Int(3, 0)
            );
            board.PlacePiece(
                new King(PlayerColor.White, new Vector2Int(4, 0)),
                new Vector2Int(4, 0)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.White, new Vector2Int(5, 0)),
                new Vector2Int(5, 0)
            );
            board.PlacePiece(
                new Knight(PlayerColor.White, new Vector2Int(6, 0)),
                new Vector2Int(6, 0)
            );
            board.PlacePiece(
                new Rook(PlayerColor.White, new Vector2Int(7, 0)),
                new Vector2Int(7, 0)
            );

            for (int x = 0; x < 8; x++)
            {
                board.PlacePiece(
                    new Pawn(PlayerColor.White, new Vector2Int(x, 1)),
                    new Vector2Int(x, 1)
                );
            }

            // Black pieces
            board.PlacePiece(
                new Rook(PlayerColor.Black, new Vector2Int(0, 7)),
                new Vector2Int(0, 7)
            );
            board.PlacePiece(
                new Knight(PlayerColor.Black, new Vector2Int(1, 7)),
                new Vector2Int(1, 7)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.Black, new Vector2Int(2, 7)),
                new Vector2Int(2, 7)
            );
            board.PlacePiece(
                new Queen(PlayerColor.Black, new Vector2Int(3, 7)),
                new Vector2Int(3, 7)
            );
            board.PlacePiece(
                new King(PlayerColor.Black, new Vector2Int(4, 7)),
                new Vector2Int(4, 7)
            );
            board.PlacePiece(
                new Bishop(PlayerColor.Black, new Vector2Int(5, 7)),
                new Vector2Int(5, 7)
            );
            board.PlacePiece(
                new Knight(PlayerColor.Black, new Vector2Int(6, 7)),
                new Vector2Int(6, 7)
            );
            board.PlacePiece(
                new Rook(PlayerColor.Black, new Vector2Int(7, 7)),
                new Vector2Int(7, 7)
            );

            for (int x = 0; x < 8; x++)
            {
                board.PlacePiece(
                    new Pawn(PlayerColor.Black, new Vector2Int(x, 6)),
                    new Vector2Int(x, 6)
                );
            }
        }

        private void InitializeGrid()
        {
            _grid.Controls.Clear();
            _grid.RowCount = 8;
            _grid.ColumnCount = 8;
            _cells = new Button[8, 8];

            for (int y = 7; y >= 0; y--) // Start from top (y=7) and go down
            {
                for (int x = 0; x < 8; x++)
                {
                    var cell = new Button
                    {
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Arial", 24, FontStyle.Bold), // Larger font
                        Tag = new Vector2Int(x, y),
                        Margin = Padding.Empty,
                        Padding = Padding.Empty,
                    };
                    cell.Click += Cell_Click;
                    _cells[x, y] = cell;
                    _grid.Controls.Add(cell, x, 7 - y); // Reverse y for display
                }
            }

            // Set equal sizing
            for (int i = 0; i < 8; i++)
            {
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
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
            if (currentState.CurrentPlayer != PlayerColor.White)
            {
                // It's the AI's turn, ignore clicks
                return;
            }

            if (_selected == null)
            {
                // First click - select piece
                if (piece != null && piece.Owner == currentState.CurrentPlayer)
                {
                    _selected = pos;
                    _legalTargets = _whiteHuman!
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
                    _whiteHuman!.SubmitMove(move);
                }
                else
                {
                    // Cancel selection
                    _selected = null;
                    _legalTargets = Array.Empty<Vector2Int>();
                    UpdateDisplay();
                }
            }
        }

        private void UpdateDisplay()
        {
            if (_gameEngine == null || _cells == null)
                return;

            var state = _gameEngine.CurrentState;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var cell = _cells[x, y];
                    var piece = state.Board.GetPieceAt(pos);

                    // Set piece symbol
                    cell.Text = piece?.Name switch
                    {
                        "King" => piece.Owner == PlayerColor.White ? "♔" : "♚",
                        "Queen" => piece.Owner == PlayerColor.White ? "♕" : "♛",
                        "Rook" => piece.Owner == PlayerColor.White ? "♖" : "♜",
                        "Bishop" => piece.Owner == PlayerColor.White ? "♗" : "♝",
                        "Knight" => piece.Owner == PlayerColor.White ? "♘" : "♞",
                        "Pawn" => piece.Owner == PlayerColor.White ? "♙" : "♟",
                        _ => "",
                    };

                    // Set colors
                    var isLight = (x + y) % 2 == 0;
                    cell.BackColor = GetCellColor(pos, isLight);
                    cell.ForeColor = piece?.Owner == PlayerColor.White ? Color.White : Color.Black;

                    // Make the piece text more visible
                    if (piece != null)
                    {
                        cell.ForeColor =
                            piece.Owner == PlayerColor.White ? Color.White : Color.Black;
                        // Add a subtle shadow effect for better visibility
                        cell.FlatAppearance.BorderSize = 0;
                    }
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

            // Highlight legal targets
            if (_legalTargets.Contains(pos))
            {
                return Color.LightGreen;
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
            _debugPanel?.AddEvent(gameEvent);

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
                _gameEngine.Undo();
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
                _gameEngine.Redo();
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
