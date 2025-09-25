using System;
using System.Drawing;
using System.Windows.Forms;
using RogueChess.Assets;

namespace MapBuilder
{
    public partial class MapBuilderForm : Form
    {
        private Panel legendPanel;
        private Panel boardPanel;
        private TableLayoutPanel layout;

        private int boardWidth;
        private int boardHeight;

        public MapBuilderForm(int width, int height)
        {
            boardWidth = width;
            boardHeight = height;

            Text = "RogueChess Map Builder";
            WindowState = FormWindowState.Maximized;

            InitMenu();
            InitLayout();
            BuildBoard(boardWidth, boardHeight);
            BuildLegend();
        }

        private void InitMenu()
        {
            var menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");

            var newItem = new ToolStripMenuItem("New");
            newItem.Click += (s, e) =>
            {
                using var sizeDialog = new BoardSizeForm();
                if (sizeDialog.ShowDialog(this) == DialogResult.OK)
                {
                    boardWidth = sizeDialog.BoardWidth;
                    boardHeight = sizeDialog.BoardHeight;
                    BuildBoard(boardWidth, boardHeight);
                }
            };

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Close();

            fileMenu.DropDownItems.Add(newItem);
            fileMenu.DropDownItems.Add(exitItem);

            menu.Items.Add(fileMenu);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void InitLayout()
        {
            layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // legend fixed
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // board fills

            legendPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            boardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.LightGray,
            };

            layout.Controls.Add(legendPanel, 0, 0);
            layout.Controls.Add(boardPanel, 1, 0);

            Controls.Add(layout);
        }

        private void BuildBoard(int width, int height)
        {
            boardPanel.Controls.Clear();
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = width,
                RowCount = height,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            };

            for (int x = 0; x < width; x++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / width));
            for (int y = 0; y < height; y++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / height));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = (x + y) % 2 == 0 ? Color.White : Color.LightBlue,
                    };
                    grid.Controls.Add(cell, x, y);
                }
            }

            boardPanel.Controls.Add(grid);
        }

        private void BuildLegend()
        {
            legendPanel.Controls.Clear();
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
            };

            foreach (var piece in ComponentRegistry.GetPieceTypes())
            {
                var btn = new Button
                {
                    Text = piece,
                    Width = 180,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                flow.Controls.Add(btn);
            }

            legendPanel.Controls.Add(flow);
        }
    }
}
