using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public class BankReconciliationStatementForm : Form
    {
        public BankReconciliationStatementForm()
        {
            Text = "Bank Reconciliation Statement";
            Width = 980;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Bank Reconciliation Statement",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20),
                ForeColor = Color.FromArgb(40, 46, 58)
            };

            var grid = new DataGridView
            {
                Location = new Point(24, 72),
                Size = new Size(900, 500),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 36,
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

            grid.Columns.Add("Item", "Item");
            grid.Columns.Add("Amount", "Amount");
            grid.Columns.Add("Notes", "Notes");

            grid.Rows.Add("Bank statement balance", "$19,240.00", "Per statement");
            grid.Rows.Add("Add: deposits in transit", "$2,100.00", "August 18");
            grid.Rows.Add("Less: outstanding checks", "($1,820.00)", "Check 2048");
            grid.Rows.Add("Adjusted bank balance", "$19,520.00", "Reconciled");
            grid.Rows.Add("Book balance", "$18,740.00", "General ledger");
            grid.Rows.Add("Difference", "$780.00", "Review required");

            Controls.Add(title);
            Controls.Add(grid);
        }
    }
}
