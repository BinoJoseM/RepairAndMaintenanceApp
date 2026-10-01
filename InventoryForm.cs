using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public class InventoryForm : Form
    {
        public InventoryForm()
        {
            Text = "Inventory";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Inventory Management",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20),
                ForeColor = Color.FromArgb(40, 46, 58)
            };

            var grid = new DataGridView
            {
                Location = new Point(24, 72),
                Size = new Size(820, 500),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(223, 230, 239),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            grid.RowTemplate.Height = 32;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(2, 0, 2, 0)
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(42, 50, 59),
                SelectionBackColor = Color.FromArgb(140, 156, 189),
                SelectionForeColor = Color.White,
                Padding = new Padding(2, 3, 2, 3),
                Font = new Font("Segoe UI", 10F)
            };

            grid.Columns.Add("Part", "Part Number");
            grid.Columns.Add("Description", "Description");
            grid.Columns.Add("Qty", "Quantity");
            grid.Columns.Add("ReorderLevel", "Reorder Level");
            grid.Columns.Add("Supplier", "Supplier");

            grid.Rows.Add("P-1001", "Air Filter", "38", "20", "Filter Depot");
            grid.Rows.Add("P-2004", "Fan Belt", "12", "18", "Moto Supply");
            grid.Rows.Add("P-3202", "Control Relay", "09", "12", "Industrial Parts");
            grid.Rows.Add("P-4100", "Valve Seal", "04", "10", "Prime Components");
            grid.Rows.Add("P-5108", "Lubricant 5L", "21", "15", "FluidPro");

            Controls.Add(title);
            Controls.Add(grid);
        }
    }
}
