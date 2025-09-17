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
                    FlatStyle = FlatStyle.Standard,
                };
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

        // Only accept clicks on a human turn
        if (!IsHumanTurn(state))
            return;

        var pos = (Vector2Int)((Button)sender!).Tag;
        var piece = state.Board.GetPieceAt(pos);

        // First click: select & highlight
        if (_selected == null)
        {
            if (piece != null && piece.Owner == state.CurrentPlayer)
            {
                _selected = pos;
                _legalTargets = _ruleset.GetLegalMoves(state, piece).Select(m => m.To).ToArray();
                RenderBoard();
                foreach (var t in _legalTargets)
                    _cells[t.x, t.y].BackColor = Color.Yellow;
            }
            return;
        }

        // Second click: try to play
        var from = _selected.Value;
        var legalMove = state
            .Board.GetAllPieces(state.CurrentPlayer)
            .SelectMany(p => _ruleset.GetLegalMoves(state, p))
            .FirstOrDefault(m => m.From == from && m.To == pos);

        var human = CurrentHuman(state);
        if (legalMove != null)
        {
            human?.SubmitMove(legalMove);
        }
        else
        {
            // allow reselection or cancel
            if (piece != null && piece.Owner == state.CurrentPlayer)
            {
                _selected = pos;
                _legalTargets = _ruleset.GetLegalMoves(state, piece).Select(m => m.To).ToArray();
                RenderBoard();
                foreach (var t in _legalTargets)
                    _cells[t.x, t.y].BackColor = Color.Yellow;
                return;
            }

            human?.SubmitMove(null);
        }

        _selected = null;
        _legalTargets = Array.Empty<Vector2Int>();
        RenderBoard();
    }

    private void RenderBoard()
    {
        if (_runner is null || _cells is null)
            return;

        var state = _runner.GetState();
        for (int y = 0; y < state.Board.Height; y++)
        {
            for (int x = 0; x < state.Board.Width; x++)
            {
                var pos = new Vector2Int(x, y);
                var tile = state.Board.GetTile(pos);
                var piece = state.Board.GetPieceAt(pos);
                var btn = _cells[x, y];

                btn.BackColor = tile switch
                {
                    SlipperyTile => Color.LightBlue,
                    ScorchedTile => Color.IndianRed,
                    _ => Color.Beige,
                };

                if ((x + y) % 2 == 1)
                    btn.BackColor = ControlPaint.Light(btn.BackColor);

                if (piece != null)
                {
                    var c = (piece.Name?.Length ?? 0) > 0 ? piece.Name![0] : '?';
                    btn.Text = c.ToString();
                    btn.ForeColor = piece.Owner == PlayerColor.White ? Color.Black : Color.Maroon;
                    btn.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
                }
                else
                {
                    btn.Text = "";
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
