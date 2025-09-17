namespace RogueChess.UI;

public enum PlayerKind
{
    Human,
    RandomAI,
}

public class GameOptions
{
    public Func<(
        ChessRogue.Core.GameState,
        ChessRogue.Core.RuleSets.IRuleSet
    )> SetupFactory { get; init; } = default!;
    public PlayerKind White { get; init; } = PlayerKind.Human;
    public PlayerKind Black { get; init; } = PlayerKind.RandomAI;
}

public class GameOptionsForm : Form
{
    private readonly ComboBox _ruleset = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
    };
    private readonly ComboBox _white = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
    };
    private readonly ComboBox _black = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
    };
    private readonly Button _ok = new() { Text = "Start", Width = 96 };
    private readonly Button _cancel = new() { Text = "Cancel", Width = 96 };

    public GameOptions Options { get; private set; } =
        new GameOptions
        {
            SetupFactory = StandardGameSetup.Create,
            White = PlayerKind.Human,
            Black = PlayerKind.RandomAI,
        };

    public GameOptionsForm()
    {
        Text = "New Game";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;

        // Populate combos
        _ruleset.Items.AddRange(["Standard", "Last Piece Standing"]);
        _ruleset.SelectedIndex = 0;

        _white.Items.AddRange(["Human", "Random AI"]);
        _black.Items.AddRange(["Human", "Random AI"]);
        _white.SelectedIndex = 0; // Human
        _black.SelectedIndex = 1; // AI

        // Layout
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 4,
            ColumnCount = 2,
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        grid.Controls.Add(
            new Label
            {
                Text = "Ruleset",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            },
            0,
            0
        );
        grid.Controls.Add(_ruleset, 1, 0);

        grid.Controls.Add(
            new Label
            {
                Text = "White",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            },
            0,
            1
        );
        grid.Controls.Add(_white, 1, 1);

        grid.Controls.Add(
            new Label
            {
                Text = "Black",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            },
            0,
            2
        );
        grid.Controls.Add(_black, 1, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };
        _ok.DialogResult = DialogResult.OK;
        _cancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(_ok);
        buttons.Controls.Add(_cancel);

        grid.Controls.Add(buttons, 0, 3);
        grid.SetColumnSpan(buttons, 2);

        Controls.Add(grid);
        ClientSize = new Size(380, 180);

        // Accept/Cancel (Enter/Esc)
        AcceptButton = _ok;
        CancelButton = _cancel;

        // Click handlers
        _ok.Click += (_, __) =>
        {
            Options = new GameOptions
            {
                SetupFactory =
                    _ruleset.SelectedIndex == 0 ? StandardGameSetup.Create : CustomGameSetup.Create,
                White = _white.SelectedIndex == 0 ? PlayerKind.Human : PlayerKind.RandomAI,
                Black = _black.SelectedIndex == 0 ? PlayerKind.Human : PlayerKind.RandomAI,
            };
            // DialogResult already set to OK via DialogResult property.
            // Closing is automatic because AcceptButton is set.
        };

        _cancel.Click += (_, __) => {
            // DialogResult already set to Cancel; form will close.
        };
    }
}
