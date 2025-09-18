using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChessRogue.Core;
using ChessRogue.Core.Board.Tiles;
using ChessRogue.Core.Events;
using ChessRogue.Core.RuleSets;
using ChessRogue.Core.Runner;

namespace RogueChess.UI;

public class BoardForm : Form
{
    private GameRunner? _runner;
    private IRuleSet? _ruleset;

    // Players
    private UIHumanController? _whiteHuman;
    private UIHumanController? _blackHuman;
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

    // loop management
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;

    public BoardForm()
    {
        Text = "Rogue Chess";
        StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(_grid);
        ClientSize = new Size(640, 660);

        // Menu: New…, quick presets
        var menu = new MenuStrip();
        var game = new ToolStripMenuItem("Game");
        var newDialog = new ToolStripMenuItem("New…", null, (_, __) => ShowNewGameDialog());
        var newStandard = new ToolStripMenuItem(
            "New – Standard (Human vs AI)",
            null,
            (_, __) =>
            {
                StartNewGame(
                    new GameOptions
                    {
                        SetupFactory = StandardGameSetup.Create,
                        White = PlayerKind.Human,
                        Black = PlayerKind.RandomAI,
                    }
                );
            }
        );
        var newCustom = new ToolStripMenuItem(
            "New – Last Piece Standing (Human vs AI)",
            null,
            (_, __) =>
            {
                StartNewGame(
                    new GameOptions
                    {
                        SetupFactory = CustomGameSetup.Create,
                        White = PlayerKind.Human,
                        Black = PlayerKind.RandomAI,
                    }
                );
            }
        );
        game.DropDownItems.Add(newDialog);
        game.DropDownItems.Add(new ToolStripSeparator());
        game.DropDownItems.Add(newStandard);
        game.DropDownItems.Add(newCustom);
        menu.Items.Add(game);
        MainMenuStrip = menu;
        Controls.Add(menu);
        menu.Dock = DockStyle.Top;

        // Start initial game (Human White vs AI Black, Standard)
        StartNewGame(
            new GameOptions
            {
                SetupFactory = StandardGameSetup.Create,
                White = PlayerKind.Human,
                Black = PlayerKind.RandomAI,
            }
        );
    }

    private void ShowNewGameDialog()
    {
        using var dlg = new GameOptionsForm();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            StartNewGame(dlg.Options);
        }
    }

    private void StartNewGame(GameOptions options)
    {
        // 1) stop previous loop & detach
        StopLoop();

        if (_runner is not null)
            _runner.OnEventPublished -= OnRunnerEvent;

        _whiteHuman?.CancelPending();
        _blackHuman?.CancelPending();

        // 2) create new runner + players
        var (state, ruleset) = options.SetupFactory();
        _ruleset = ruleset;

        _whiteHuman = options.White == PlayerKind.Human ? new UIHumanController(_ruleset) : null;
        _blackHuman = options.Black == PlayerKind.Human ? new UIHumanController(_ruleset) : null;

        _whitePlayer =
            options.White == PlayerKind.Human ? _whiteHuman! : new RandomAIController(_ruleset);

        _blackPlayer =
            options.Black == PlayerKind.Human ? _blackHuman! : new RandomAIController(_ruleset);

        _runner = new GameRunner(state, _whitePlayer, _blackPlayer, _ruleset);
        _runner.OnEventPublished += OnRunnerEvent;

        // 3) (re)build board UI and render
        InitBoard(state.Board.Width, state.Board.Height);
        RenderBoard();

        // 4) start fresh loop
        StartLoop();
    }

    private void StartLoop()
    {
        _loopCts = new CancellationTokenSource();
        _loopTask = Task.Run(() => GameLoop(_loopCts.Token));
    }

    private void StopLoop()
    {
        try
        {
            _loopCts?.Cancel();
            _whiteHuman?.CancelPending();
            _blackHuman?.CancelPending();
            _loopTask?.Wait(250);
        }
        catch
        { /* ignore */
        }
        finally
        {
            _loopCts?.Dispose();
            _loopCts = null;
            _loopTask = null;
        }
    }

    private void OnRunnerEvent(GameEvent _)
    {
        if (!IsHandleCreated || IsDisposed)
            return;
        if (InvokeRequired)
            BeginInvoke((Action)RenderBoard);
        else
            RenderBoard();
    }

    private void InitBoard(int w, int h)
    {
        _grid.SuspendLayout();
        _grid.Controls.Clear();
        _grid.RowStyles.Clear();
        _grid.ColumnStyles.Clear();

        _grid.RowCount = h;
        _grid.ColumnCount = w;

        for (int r = 0; r < h; r++)
            _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / h));
        for (int c = 0; c < w; c++)
            _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / w));

        _cells = new Button[w, h];

        // Invert rows so (0,0) renders bottom-left
        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                var btn = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Beige,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Tag = new Vector2Int(x, y);
                btn.Click += OnCellClick;

                _grid.Controls.Add(btn, x, (h - 1) - y);
                _cells[x, y] = btn;
            }
        }

        _grid.ResumeLayout();
    }

    private bool IsHumanTurn(ChessRogue.Core.GameState state)
    {
        if (_ruleset is null)
            return false;
        if (state.CurrentPlayer == PlayerColor.White)
            return _whiteHuman is not null;
        return _blackHuman is not null;
    }

    private UIHumanController? CurrentHuman(ChessRogue.Core.GameState state)
    {
        return state.CurrentPlayer == PlayerColor.White ? _whiteHuman : _blackHuman;
    }

    private void OnCellClick(object? sender, EventArgs e)
    {
        if (_runner is null || _ruleset is null || _cells is null)
            return;

        var state = _runner.GetState();

        if (!IsHumanTurn(state))
            return;

        var pos = (Vector2Int)((Button)sender!).Tag;
        var piece = state.Board.GetPieceAt(pos);

        // First click: select
        if (_selected == null)
        {
            if (piece != null && piece.Owner == state.CurrentPlayer)
            {
                _selected = pos;
                RenderBoard();
            }
            return;
        }

        // Second click: attempt move
        var from = _selected.Value;
        var legalMove = state
            .Board.GetAllPieces(state.CurrentPlayer)
            .SelectMany(p => _ruleset.GetLegalMoves(state, p))
            .FirstOrDefault(m => m.From == from && m.To == pos);

        var human = CurrentHuman(state);
        if (legalMove != null)
        {
            _lastMoveFrom = legalMove.From;
            _lastMoveTo = legalMove.To;

            human?.SubmitMove(legalMove);
        }
        else
        {
            if (piece != null && piece.Owner == state.CurrentPlayer)
            {
                _selected = pos;
                RenderBoard();
                return;
            }
            human?.SubmitMove(null);
        }

        _selected = null;
        RenderBoard();
    }

    private void RenderBoard()
    {
        if (_runner is null || _cells is null)
            return;

        var state = _runner.GetState();

        // Track the last move from history
        _lastMoveFrom = null;
        _lastMoveTo = null;
        if (state.MoveHistory.Count > 0)
        {
            var last = state.MoveHistory[^1]; // C# index-from-end
            _lastMoveFrom = last.From;
            _lastMoveTo = last.To;
        }

        for (int y = 0; y < state.Board.Height; y++)
        {
            for (int x = 0; x < state.Board.Width; x++)
            {
                var pos = new Vector2Int(x, y);
                var tile = state.Board.GetTile(pos);
                var piece = state.Board.GetPieceAt(pos);
                var btn = _cells[x, y];

                // Base colors
                Color lightBrown = Color.FromArgb(240, 217, 181);
                Color darkBrown = Color.FromArgb(181, 136, 99);
                var baseColor = (x + y) % 2 == 0 ? lightBrown : darkBrown;

                if (tile is SlipperyTile)
                    baseColor = Color.LightBlue;
                else if (tile is ScorchedTile)
                    baseColor = Color.IndianRed;

                // Highlights
                if (_selected.HasValue && _selected.Value == pos)
                {
                    baseColor = ControlPaint.Dark(baseColor, 0.3f); // actively selected
                }
                else if (
                    (_lastMoveFrom.HasValue && _lastMoveFrom.Value == pos)
                    || (_lastMoveTo.HasValue && _lastMoveTo.Value == pos)
                )
                {
                    baseColor = ControlPaint.Dark(baseColor, 0.2f); // last move
                }

                btn.BackColor = baseColor;

                // Piece rendering
                if (piece != null)
                {
                    var color = piece.Owner == PlayerColor.White ? "w" : "b";
                    var name = piece.Name.ToLower();
                    var key = $"{name}-{color}";

                    var size = Math.Min(btn.Width, btn.Height) - 3;
                    var bmp = PieceImageCache.RenderSvg(key, size);

                    btn.Text = "";
                    btn.Image = bmp;
                    btn.ImageAlign = ContentAlignment.MiddleCenter;
                    btn.BackgroundImageLayout = ImageLayout.Zoom;
                }
                else
                {
                    btn.Text = "";
                    btn.Image = null;
                }
            }
        }
    }

    private void GameLoop(CancellationToken token)
    {
        if (_runner is null || _ruleset is null)
            return;

        while (!token.IsCancellationRequested)
        {
            var state = _runner.GetState();
            if (_ruleset.IsGameOver(state, out _))
                break;
            _runner.RunTurn();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        StopLoop();
        if (_runner is not null)
            _runner.OnEventPublished -= OnRunnerEvent;
    }
}
