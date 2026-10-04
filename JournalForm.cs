using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RepairAndMaintenanceApp.DataLayer;

namespace RepairAndMaintenanceApp
{
    public class JournalForm : Form
    {
        public JournalForm()
        {
            Text = "Journal";
            Width = 980;
            Height = 820;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Journal",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20),
                ForeColor = Color.FromArgb(40, 46, 58)
            };

            var searchPanel = new Panel
            {
                Location = new Point(24, 62),
                Size = new Size(900, 58),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12, 10, 12, 10),
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
                Width = 150,
                Location = new Point(62, 15),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMMM yyyy",
                ShowUpDown = true
            };

            var particularsLabel = new Label
            {
                Text = "Particulars",
                AutoSize = true,
                Location = new Point(232, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var particularsBox = new ComboBox
            {
                Width = 190,
                Location = new Point(306, 14),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            particularsBox.Items.Add("All particulars");
            particularsBox.Items.AddRange(JournalTransactionDataAccess.GetDistinctParticulars().Cast<object>().ToArray());
            particularsBox.SelectedIndex = 0;

            var searchButton = CreateActionButton("Search", 512, 13, 90, Color.FromArgb(32, 74, 140), Color.White);
            var clearButton = CreateActionButton("Clear", 612, 13, 80, Color.FromArgb(232, 236, 240), Color.FromArgb(60, 72, 84));

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.Black,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                ColumnHeadersHeight = 32,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            };
            grid.RowTemplate.Height = 30;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(2, 0, 2, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.Black,
                Padding = new Padding(2, 3, 2, 3),
                Font = new Font("Segoe UI", 10F),
                NullValue = string.Empty
            };

            grid.Columns.Add("Date", "Date");
            grid.Columns.Add("Particulars", "Particulars");
            grid.Columns.Add("Debit", "Amount (Rs)");
            grid.Columns.Add("Credit", "Amount (Rs)");
            grid.Columns["Date"]!.FillWeight = 18;
            grid.Columns["Particulars"]!.FillWeight = 48;
            grid.Columns["Debit"]!.FillWeight = 17;
            grid.Columns["Credit"]!.FillWeight = 17;
            grid.Columns["Debit"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["Credit"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var statementTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.White
            };
            var statementPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            statementPanel.Controls.Add(grid);
            statementPanel.Controls.Add(statementTitle);

            var balanceGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.FromArgb(223, 230, 239),
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 32 }
            };
            balanceGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(2, 0, 2, 0)
            };
            balanceGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(42, 50, 59),
                SelectionBackColor = Color.FromArgb(140, 156, 189),
                SelectionForeColor = Color.White,
                Padding = new Padding(2, 3, 2, 3),
                Font = new Font("Segoe UI", 9F)
            };
            balanceGrid.Columns.Add("Particulars", "Particulars");
            balanceGrid.Columns.Add("OpeningDebit", "Opening Dr (₹)");
            balanceGrid.Columns.Add("OpeningCredit", "Opening Cr (₹)");
            balanceGrid.Columns.Add("MonthlyDebit", "Month Dr (₹)");
            balanceGrid.Columns.Add("MonthlyCredit", "Month Cr (₹)");
            balanceGrid.Columns.Add("ClosingDebit", "Closing Dr (₹)");
            balanceGrid.Columns.Add("ClosingCredit", "Closing Cr (₹)");
            balanceGrid.Columns["Particulars"]!.FillWeight = 30;
            foreach (var columnName in new[] { "OpeningDebit", "OpeningCredit", "MonthlyDebit", "MonthlyCredit", "ClosingDebit", "ClosingCredit" })
            {
                balanceGrid.Columns[columnName]!.FillWeight = 14;
                balanceGrid.Columns[columnName]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            var tabs = new TabControl
            {
                Location = new Point(24, 134),
                Size = new Size(900, 600),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            var transactionsTab = new TabPage("Journal Entries");
            var balancesTab = new TabPage("Opening / Closing Balances");
            transactionsTab.Controls.Add(statementPanel);
            balancesTab.Controls.Add(balanceGrid);
            tabs.TabPages.Add(transactionsTab);
            tabs.TabPages.Add(balancesTab);

            void LoadTransactions()
            {
                var selectedMonth = monthPicker.Value;
                var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                statementTitle.Text = $"Daily Transactions for the month of {monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}";

                var selectedParticulars = particularsBox.SelectedIndex > 0
                    ? particularsBox.SelectedItem?.ToString()
                    : null;
                var transactions = JournalTransactionDataAccess.GetAll(selectedMonth, selectedParticulars);
                var particularBalances = JournalTransactionDataAccess.GetMonthlyBalances(selectedMonth, selectedParticulars);

                grid.Rows.Clear();

                var openingNet = particularBalances.Sum(balance => balance.OpeningDebit - balance.OpeningCredit);
                var monthlyDebit = particularBalances.Sum(balance => balance.MonthlyDebit);
                var monthlyCredit = particularBalances.Sum(balance => balance.MonthlyCredit);
                var totalDebit = Math.Max(openingNet, 0m) + monthlyDebit;
                var totalCredit = Math.Max(-openingNet, 0m) + monthlyCredit;

                AddStatementRow(
                    monthStart.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    "Opening Balance",
                    Math.Max(openingNet, 0m),
                    Math.Max(-openingNet, 0m),
                    true);

                foreach (var transaction in transactions)
                {
                    AddStatementRow(
                        transaction.EntryDate.HasValue ? FormatDate(transaction.EntryDate) : string.Empty,
                        transaction.Particulars,
                        transaction.Debit,
                        transaction.Credit,
                        false);
                }

                AddStatementRow(string.Empty, "Total", totalDebit, totalCredit, true);

                var closingDifference = totalDebit - totalCredit;
                var closingDebit = Math.Max(-closingDifference, 0m);
                var closingCredit = Math.Max(closingDifference, 0m);
                AddStatementRow(
                    string.Empty,
                    $"Closing Balance as on {monthEnd.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)}",
                    closingDebit,
                    closingCredit,
                    true);
                AddStatementRow(
                    string.Empty,
                    "Grand Total",
                    totalDebit + closingDebit,
                    totalCredit + closingCredit,
                    true);

                balanceGrid.Rows.Clear();
                foreach (var balance in particularBalances)
                {
                    balanceGrid.Rows.Add(
                        balance.Particulars,
                        FormatAmount(balance.OpeningDebit),
                        FormatAmount(balance.OpeningCredit),
                        FormatAmount(balance.MonthlyDebit),
                        FormatAmount(balance.MonthlyCredit),
                        FormatAmount(balance.ClosingDebit),
                        FormatAmount(balance.ClosingCredit));
                }
            }

            void AddStatementRow(string date, string particulars, decimal debit, decimal credit, bool isSummary)
            {
                var rowIndex = grid.Rows.Add(date, particulars, FormatStatementAmount(debit), FormatStatementAmount(credit));
                if (isSummary)
                {
                    grid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
            }

            searchButton.Click += (_, _) => LoadTransactions();
            clearButton.Click += (_, _) =>
            {
                monthPicker.Value = DateTime.Today;
                particularsBox.SelectedIndex = 0;
                LoadTransactions();
            };

            searchPanel.Controls.Add(monthLabel);
            searchPanel.Controls.Add(monthPicker);
            searchPanel.Controls.Add(particularsLabel);
            searchPanel.Controls.Add(particularsBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(clearButton);

            Controls.Add(title);
            Controls.Add(searchPanel);
            Controls.Add(tabs);

            Resize += (_, _) =>
            {
                searchPanel.Width = Math.Max(0, ClientSize.Width - 48);
                tabs.Width = Math.Max(0, ClientSize.Width - 48);
                tabs.Height = Math.Max(120, ClientSize.Height - 158);
            };

            LoadTransactions();
        }

        private static string FormatDate(DateTime? date)
        {
            return date?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string FormatAmount(decimal amount)
        {
            return amount == 0m
                ? string.Empty
                : $"₹{amount.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-IN"))}";
        }

        private static string FormatStatementAmount(decimal amount)
        {
            return amount == 0m
                ? "-"
                : amount.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-IN"));
        }

        private static Button CreateActionButton(string text, int x, int y, int width, Color backColor, Color foreColor)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
