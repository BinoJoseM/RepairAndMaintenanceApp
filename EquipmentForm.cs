using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public class EquipmentForm : Form
    {
        public EquipmentForm()
        {
            Text = "Equipment";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Equipment Register",
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

            grid.Columns.Add("Asset", "Asset ID");
            grid.Columns.Add("Name", "Asset Name");
            grid.Columns.Add("Location", "Location");
            grid.Columns.Add("Status", "Status");
            grid.Columns.Add("LastService", "Last Service");

            grid.Rows.Add("EQ-101", "HVAC Unit 7", "Building A", "Operational", "2026-08-14");
            grid.Rows.Add("EQ-203", "Generator B", "Plant 2", "Maintenance", "2026-08-21");
            grid.Rows.Add("EQ-312", "Dock Lift 3", "Warehouse", "Operational", "2026-08-16");
            grid.Rows.Add("EQ-460", "Boiler Unit 2", "Utility Room", "Warning", "2026-08-10");
            grid.Rows.Add("EQ-520", "Chiller System", "Tower East", "Operational", "2026-08-17");

            Controls.Add(title);
            Controls.Add(grid);
        }
    }
}
