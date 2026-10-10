using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Text = "Accounting Workspace";
            Width = 1100;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(240, 243, 247);
            Font = new Font("Segoe UI", 10F);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(20, 58, 112)
            };

            var title = new Label
            {
                Text = "August 2026 Financial Workspace",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 18)
            };

            header.Controls.Add(title);
            Controls.Add(header);

            var nav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(32, 45, 54)
            };

            var buttons = new[]
            {
                new { Text = "Journal", FormType = typeof(JournalForm) },
                new { Text = "Bank Reconciliation Statement", FormType = typeof(BankReconciliationStatementForm) },
                new { Text = "Ledger(Expenses)", FormType = typeof(RecordExpenseForm) },
                new { Text = "Ledger (Income)", FormType = typeof(RecordIncomeForm) },
                new { Text = "Category Master", FormType = typeof(CategoryMasterForm) },
                new { Text = "Reports", FormType = typeof(ReportForm) },
                new { Text = "BS", FormType = typeof(BalanceSheetForm) }
            };

            var y = 24;
            foreach (var item in buttons)
            {
                var button = new Button
                {
                    Text = item.Text,
                    Width = 170,
                    Height = 38,
                    Location = new Point(22, y),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(46, 62, 74),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                button.FlatAppearance.BorderSize = 0;
                button.Click += (_, _) =>
                {
                    var form = (Form)Activator.CreateInstance(item.FormType)!;
                    form.Show();
                };

                nav.Controls.Add(button);
                y += 52;
            }

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 243, 247),
                Padding = new Padding(24)
            };

            var introTitle = new Label
            {
                Text = "Financial Overview",
                ForeColor = Color.FromArgb(25, 40, 60),
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(18, 22)
            };

            var introText = new Label
            {
                Text = "Use the tabs below to review the accounting records and financial statements from the August 2026 workbook.",
                ForeColor = Color.FromArgb(80, 94, 108),
                Font = new Font("Segoe UI", 11F),
                AutoSize = true,
                MaximumSize = new Size(700, 0),
                Location = new Point(18, 72)
            };

            var summary = new DataGridView
            {
                Location = new Point(18, 120),
                Size = new Size(760, 420),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            summary.Columns.Add("Module", "Module");
            summary.Columns.Add("Status", "Status");
            summary.Columns.Add("Value", "Value");
            summary.Rows.Add("Journal", "Current", "$24,560");
            summary.Rows.Add("Bank Reconciliation Statement", "Matched", "$18,740");
            summary.Rows.Add("Ledger(Expenses)", "Reviewed", "$9,420");
            summary.Rows.Add("Ledger (Income)", "Reviewed", "$15,130");
            summary.Rows.Add("BS", "Updated", "$62,400");

            content.Controls.Add(introTitle);
            content.Controls.Add(introText);
            content.Controls.Add(summary);

            Controls.Add(nav);
            Controls.Add(content);
        }
    }
}