using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public class WorkOrdersForm : Form
    {
        public WorkOrdersForm()
        {
            Text = "Work Orders";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Work Orders",
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

            grid.Columns.Add("Id", "WO #");
            grid.Columns.Add("Customer", "Customer");
            grid.Columns.Add("Asset", "Asset");
            grid.Columns.Add("Priority", "Priority");
            grid.Columns.Add("Status", "Status");

            grid.Rows.Add("WO-1042", "Apex Motors", "HVAC Unit 7", "High", "In Progress");
            grid.Rows.Add("WO-1047", "Greenfield Clinic", "Generator B", "Critical", "Waiting");
            grid.Rows.Add("WO-1049", "Harbor Logistics", "Dock Lift 3", "Medium", "Scheduled");
            grid.Rows.Add("WO-1051", "Nexa Foods", "Chiller System", "High", "In Progress");
            grid.Rows.Add("WO-1058", "Metro Retail", "Boiler Unit 2", "Low", "Closed");

            Controls.Add(title);
            Controls.Add(grid);
        }
    }
}
