using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class MonthlyIncomeExpenseSummaryForm : Form
    {
        public MonthlyIncomeExpenseSummaryForm()
        {
            Text = "Monthly Income and Expense Summary";
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "Monthly Income and Expense Summary",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 24),
                ForeColor = Color.FromArgb(25, 35, 46)
            };

            var filterPanel = new Panel
            {
                Location = new Point(24, 68),
                Height = 58,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var monthLabel = new Label
            {
                Text = "Month",
                AutoSize = true,
                Location = new Point(14, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var monthPicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMMM yyyy",
                ShowUpDown = true,
                Width = 160,
                Location = new Point(68, 14),
                Font = new Font("Segoe UI", 9F)
            };

            var searchButton = new Button
            {
                Text = "Show Summary",
                Width = 120,
                Height = 30,
                Location = new Point(244, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            searchButton.FlatAppearance.BorderSize = 0;

            var grid = new DataGridView
            {
                Location = new Point(24, 140),
                Size = new Size(900, 430),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(220, 226, 232),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 32 },
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F)
            };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(38, 46, 57),
                SelectionBackColor = Color.FromArgb(221, 232, 244),
                SelectionForeColor = Color.FromArgb(38, 46, 57),
                Padding = new Padding(6, 0, 6, 0)
            };
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.Columns.Add("Particulars", "Particulars");
            grid.Columns.Add("Debit", "Debit (₹)");
            grid.Columns.Add("Credit", "Credit (₹)");
            grid.Columns.Add("Type", "Type");
            grid.Columns["Particulars"]!.FillWeight = 50;
            grid.Columns["Debit"]!.FillWeight = 18;
            grid.Columns["Credit"]!.FillWeight = 18;
            grid.Columns["Type"]!.FillWeight = 14;
            grid.Columns["Debit"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["Credit"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            void LoadSummary()
            {
                var month = monthPicker.Value;
                title.Text = $"Monthly Income and Expense Summary - {month.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}";
                grid.Rows.Clear();

                foreach (var item in BalanceSheetService.GetMonthlyIncomeExpenseSummary(month))
                {
                    var rowIndex = grid.Rows.Add(
                        item.Particulars,
                        FormatAmount(item.Debit),
                        FormatAmount(item.Credit),
                        item.LineType);

                    if (string.Equals(item.LineType, "Total", StringComparison.OrdinalIgnoreCase))
                    {
                        grid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                        grid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(20, 29, 38);
                    }
                }
            }

            searchButton.Click += (_, _) => LoadSummary();
            monthPicker.ValueChanged += (_, _) => LoadSummary();

            filterPanel.Controls.Add(monthLabel);
            filterPanel.Controls.Add(monthPicker);
            filterPanel.Controls.Add(searchButton);
            Controls.Add(title);
            Controls.Add(filterPanel);
            Controls.Add(grid);

            Resize += (_, _) =>
            {
                filterPanel.Width = Math.Max(0, ClientSize.Width - 48);
                grid.Width = Math.Max(0, ClientSize.Width - 48);
                grid.Height = Math.Max(120, ClientSize.Height - 164);
            };

            LoadSummary();
        }

        private static string FormatAmount(decimal amount)
        {
            return amount == 0m
                ? string.Empty
                : amount.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-IN"));
        }
    }
}
