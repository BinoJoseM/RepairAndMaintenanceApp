using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public class ReportsForm : Form
    {
        public ReportsForm()
        {
            Text = "Reports";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Operational Reports",
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

            grid.Columns.Add("Report", "Report");
            grid.Columns.Add("Period", "Period");
            grid.Columns.Add("Owner", "Owner");
            grid.Columns.Add("Status", "Status");

            grid.Rows.Add("Service Completion", "August 2026", "Operations", "Ready");
            grid.Rows.Add("Inventory Turnover", "August 2026", "Procurement", "Ready");
            grid.Rows.Add("Preventive Schedule", "September 2026", "Maintenance", "Draft");
            grid.Rows.Add("Customer SLA Review", "Q3 2026", "Support", "Ready");
            grid.Rows.Add("Equipment Downtime", "August 2026", "Engineering", "Review");

            Controls.Add(title);
            Controls.Add(grid);
        }
    }
}
