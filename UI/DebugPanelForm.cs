using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RogueChess.Engine.Events;

namespace RogueChess.UI
{
    /// <summary>
    /// Debug panel that shows GameEvents from the canonical pipeline.
    /// This helps developers understand what's happening in the game engine.
    /// </summary>
    public partial class DebugPanelForm : Form
    {
        private ListBox _eventListBox = null!;
        private TextBox _eventDetailsTextBox = null!;
        private Button _clearButton = null!;
        private CheckBox _autoScrollCheckBox = null!;

        public DebugPanelForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Rogue Chess - Debug Panel";
            Size = new Size(600, 400);
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
            var controlPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
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

            // Set column widths
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        }

        /// <summary>
        /// Add a new GameEvent to the debug panel.
        /// </summary>
        public void AddEvent(GameEvent gameEvent)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<GameEvent>(AddEvent), gameEvent);
                return;
            }

            var eventText = FormatEventForList(gameEvent);
            _eventListBox.Items.Add(eventText);

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
        }

        private void EventListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_eventListBox.SelectedItem is string selectedText)
            {
                // Extract event ID from the selected text
                var parts = selectedText.Split('|');
                if (parts.Length > 0 && Guid.TryParse(parts[0].Trim(), out var eventId))
                {
                    // Find the corresponding GameEvent and show details
                    // For now, we'll show the selected text as details
                    _eventDetailsTextBox.Text = selectedText;
                }
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            ClearEvents();
        }

        private string FormatEventForList(GameEvent gameEvent)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var playerAction = gameEvent.IsPlayerAction ? "Player" : "System";
            var type = gameEvent.Type.ToString();
            var payload = FormatPayload(gameEvent.Payload);

            return $"{gameEvent.Id} | {timestamp} | {playerAction} | {type} | {payload}";
        }

        private string FormatPayload(object? payload)
        {
            if (payload == null)
                return "null";

            return payload switch
            {
                MovePayload movePayload =>
                    $"Move: {movePayload.Piece.Name} from {movePayload.From} to {movePayload.To}",
                CapturePayload capturePayload =>
                    $"Capture: {capturePayload.Target.Name} at {capturePayload.Target.Position}",
                TileChangePayload tilePayload =>
                    $"Tile Change: {tilePayload.Position} -> {tilePayload.NewTile.GetType().Name}",
                StatusApplyPayload statusPayload =>
                    $"Status: {statusPayload.Effect.GetType().Name} on {statusPayload.Target.Name}",
                ForcedSlidePayload slidePayload =>
                    $"Forced Slide: {slidePayload.Piece.Name} from {slidePayload.From} to {slidePayload.To}",
                // Note: These payload types are defined in the Engine but not yet implemented
                // For now, we'll handle them generically
                _ when payload.GetType().Name.Contains("Destroy") => $"Destroy: {payload}",
                _ when payload.GetType().Name.Contains("Tick") => $"Status Tick: {payload}",
                _ when payload.GetType().Name.Contains("Turn") => $"Turn: {payload}",
                _ => payload.ToString() ?? "Unknown",
            };
        }
    }
}
