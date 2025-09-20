using System.Drawing;
using System.Windows.Forms;

namespace RogueChess.UI
{
    public partial class LegendForm : Form
    {
        public LegendForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Rogue Chess - Legend";
            Size = new Size(500, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "Rogue Chess Legend",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            var tilesLabel = new Label
            {
                Text = "Special Tiles:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 50)
            };

            // Create colored indicators for tiles
            var scorchedIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 80),
                BackColor = Color.FromArgb(255, 200, 200) // Light red
            };
            var scorchedLabel = new Label
            {
                Text = "Scorched Tile - Applies burning status to pieces that enter or start their turn on it",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 85),
                MaximumSize = new Size(400, 0)
            };

            var slipperyIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 110),
                BackColor = Color.FromArgb(200, 200, 255) // Light blue
            };
            var slipperyLabel = new Label
            {
                Text = "Slippery Tile - Forces pieces to slide one extra step in their movement direction",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 115),
                MaximumSize = new Size(400, 0)
            };

            var guardianIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 140),
                BackColor = Color.FromArgb(200, 255, 200) // Light green
            };
            var guardianLabel = new Label
            {
                Text = "Guardian Tile - Protects pieces from capture (absorbs damage instead)",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 145),
                MaximumSize = new Size(400, 0)
            };

            var piecesLabel = new Label
            {
                Text = "Piece Decorators:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 180)
            };

            // Create colored indicators for piece decorators
            var explodingIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 210),
                BackColor = Color.FromArgb(255, 200, 100) // Orange
            };
            var explodingLabel = new Label
            {
                Text = "Orange Border - Exploding Piece - Explodes when captured, affecting surrounding tiles",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 215),
                MaximumSize = new Size(400, 0)
            };

            var statusIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 240),
                BackColor = Color.FromArgb(255, 100, 100) // Red
            };
            var statusLabel = new Label
            {
                Text = "Red Dot (●) - Status Effect Piece - Can carry and process status effects like Burning",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 245),
                MaximumSize = new Size(400, 0)
            };

            var martyrIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 270),
                BackColor = Color.FromArgb(100, 100, 255) // Blue
            };
            var martyrLabel = new Label
            {
                Text = "Blue Cross (✚) - Martyr Piece - Sacrifices itself to protect adjacent friendly pieces",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 275),
                MaximumSize = new Size(400, 0)
            };

            var statusEffectsLabel = new Label
            {
                Text = "Status Effects:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 310)
            };

            var burningIndicator = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(20, 340),
                BackColor = Color.FromArgb(255, 150, 150) // Light red
            };
            var burningLabel = new Label
            {
                Text = "Burning - Piece takes damage each turn and may be destroyed\n" +
                       "Status effects are applied by special tiles (like Scorched) or other pieces",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(50, 345),
                MaximumSize = new Size(400, 0)
            };

            var gameplayLabel = new Label
            {
                Text = "Game Modes:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 380)
            };

            var gameplayInfo = new Label
            {
                Text = "♟️ Standard Chess - Traditional chess with standard pieces and no special effects\n" +
                       "🎲 Random Chess - Chaotic chess with random board size, pieces, decorators, and special tiles",
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(20, 410),
                MaximumSize = new Size(450, 0)
            };

            var closeButton = new Button
            {
                Text = "Close",
                Size = new Size(80, 30),
                Location = new Point(200, 480),
                DialogResult = DialogResult.OK
            };

            panel.Controls.AddRange(new Control[] 
            { 
                titleLabel, 
                tilesLabel, 
                scorchedIndicator, scorchedLabel,
                slipperyIndicator, slipperyLabel,
                guardianIndicator, guardianLabel,
                piecesLabel, 
                explodingIndicator, explodingLabel,
                statusIndicator, statusLabel,
                martyrIndicator, martyrLabel,
                statusEffectsLabel,
                burningIndicator, burningLabel,
                gameplayLabel, 
                gameplayInfo, 
                closeButton 
            });

            Controls.Add(panel);
        }
    }
}
