using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RogueChess.Engine;
using RogueChess.Engine.Events;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.UI
{
    /// <summary>
    /// Debug panel that shows GameEvents from the canonical pipeline.
    /// Also includes live piece values for both players.
    /// </summary>
    public partial class DebugPanelForm : Form
    {
        private ListBox _eventListBox = null!;
        private TextBox _eventDetailsTextBox = null!;
        private Button _clearButton = null!;
        private CheckBox _autoScrollCheckBox = null!;
        private Label _valueSummaryLabel = null!;

        private GameState? _latestState;

        public DebugPanelForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Rogue Chess - Debug Panel";
            Size = new Size(650, 450);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(100, 100);

            // Main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
            };
            Controls.Add(mainPanel);

            // Event list (left side)
            var eventListPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
            };
            mainPanel.Controls.Add(eventListPanel, 0, 0);

            var eventListLabel = new Label
            {
                Text = "Game Events:",
                Dock = DockStyle.Top,
                Height = 20,
            };
            eventListPanel.Controls.Add(eventListLabel);

            _eventListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                SelectionMode = SelectionMode.One,
            };
            _eventListBox.SelectedIndexChanged += EventListBox_SelectedIndexChanged;
            eventListPanel.Controls.Add(_eventListBox);

            // Event details (right side)
            var detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
            };
            mainPanel.Controls.Add(detailsPanel, 1, 0);

            var detailsLabel = new Label
            {
                Text = "Event Details:",
                Dock = DockStyle.Top,
                Height = 20,
            };
            detailsPanel.Controls.Add(detailsLabel);

            _eventDetailsTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical,
            };
            detailsPanel.Controls.Add(_eventDetailsTextBox);

            // Control panel (bottom)
            var controlPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
            Controls.Add(controlPanel);

            _clearButton = new Button
            {
                Text = "Clear Events",
                Location = new Point(10, 10),
                Size = new Size(100, 25),
            };
            _clearButton.Click += ClearButton_Click;
            controlPanel.Controls.Add(_clearButton);

            _autoScrollCheckBox = new CheckBox
            {
                Text = "Auto-scroll to latest",
                Location = new Point(120, 12),
                Size = new Size(150, 25),
                Checked = true,
            };
            controlPanel.Controls.Add(_autoScrollCheckBox);

            _valueSummaryLabel = new Label
            {
                Text = "White Value: 0 | Black Value: 0",
                Location = new Point(300, 15),
                AutoSize = true,
                Font = new Font("Consolas", 9, FontStyle.Bold),
            };
            controlPanel.Controls.Add(_valueSummaryLabel);

            // Set column widths
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        }

        /// <summary>
        /// Add a new GameEvent to the debug panel.
        /// </summary>
        public void AddEvent(GameEvent gameEvent, GameState state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<GameEvent, GameState>(AddEvent), gameEvent, state);
                return;
            }

            _latestState = state;

            var eventText = FormatEventForList(gameEvent);
            _eventListBox.Items.Add(eventText);

            UpdatePieceValues(state);

            if (_autoScrollCheckBox.Checked)
            {
                _eventListBox.SelectedIndex = _eventListBox.Items.Count - 1;
                _eventListBox.TopIndex = _eventListBox.Items.Count - 1;
            }
        }

        /// <summary>
        /// Clear all events from the debug panel.
        /// </summary>
        public void ClearEvents()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearEvents));
                return;
            }

            _eventListBox.Items.Clear();
            _eventDetailsTextBox.Clear();
            _valueSummaryLabel.Text = "White Value: 0 | Black Value: 0";
        }

        private void EventListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_eventListBox.SelectedItem is string selectedText)
            {
                _eventDetailsTextBox.Text = selectedText;
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e) => ClearEvents();

        private string FormatEventForList(GameEvent gameEvent)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var playerAction = gameEvent.IsPlayerAction ? "Player" : "System";
            var type = gameEvent.Type.ToString();
            var payload = FormatPayload(gameEvent.Payload);

            // Cleaner: no GUID in the list
            return $"{timestamp} | {playerAction} | {type} | {payload}";
        }

        private string FormatPayload(object? payload)
        {
            if (payload == null)
                return "null";

            return payload switch
            {
                MovePayload mv => $"Move: {mv.Piece.Name} {mv.From}→{mv.To}",
                CapturePayload cap => $"Capture: {cap.Target.Name} at {cap.Target.Position}",
                TileChangePayload tile => $"Tile {tile.Position} → {tile.NewTile.GetType().Name}",
                StatusApplyPayload status =>
                    $"Status {status.Effect.GetType().Name} on {status.Target.Name}",
                ForcedSlidePayload slide => $"Slide {slide.Piece.Name} {slide.From}→{slide.To}",
                _ => payload.ToString() ?? "Unknown",
            };
        }

        private void UpdatePieceValues(GameState state)
        {
            var whiteValue = state
                .Board.GetAllPieces(PlayerColor.White)
                .Sum(p => PieceValueCalculator.GetTotalValue(p));

            var blackValue = state
                .Board.GetAllPieces(PlayerColor.Black)
                .Sum(p => PieceValueCalculator.GetTotalValue(p));

            _valueSummaryLabel.Text = $"White Value: {whiteValue} | Black Value: {blackValue}";
        }
    }
}
