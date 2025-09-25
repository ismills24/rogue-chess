using System;
using System.Windows.Forms;

namespace MapBuilder
{
    public partial class BoardSizeForm : Form
    {
        private NumericUpDown widthInput;
        private NumericUpDown heightInput;
        private Button okButton;
        private Button cancelButton;

        public int BoardWidth => (int)widthInput.Value;
        public int BoardHeight => (int)heightInput.Value;

        public BoardSizeForm()
        {
            Text = "New Board";
            Width = 300;
            Height = 150;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 2,
                Padding = new Padding(10),
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            table.Controls.Add(
                new Label
                {
                    Text = "Width:",
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                },
                0,
                0
            );
            widthInput = new NumericUpDown
            {
                Minimum = 4,
                Maximum = 16,
                Value = 8,
                Dock = DockStyle.Fill,
            };
            table.Controls.Add(widthInput, 1, 0);

            table.Controls.Add(
                new Label
                {
                    Text = "Height:",
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                },
                0,
                1
            );
            heightInput = new NumericUpDown
            {
                Minimum = 4,
                Maximum = 16,
                Value = 8,
                Dock = DockStyle.Fill,
            };
            table.Controls.Add(heightInput, 1, 1);

            okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Fill,
            };
            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Fill,
            };
            table.Controls.Add(okButton, 0, 2);
            table.Controls.Add(cancelButton, 1, 2);

            Controls.Add(table);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
    }
}
