using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RepairAndMaintenanceApp.Entities;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class BalanceSheetForm : Form
    {
        public BalanceSheetForm()
        {
            Text = "Balance Sheet";
            Width = 980;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            MinimumSize = new Size(980, 620);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var now = DateTime.Now;
            var sheetTitle = new Label
            {
                Text = "Balance Sheet of the " + now.ToString("MMMM yyyy"),
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 24),
                ForeColor = Color.FromArgb(25, 35, 46),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft
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

            var statementLabel = new Label
            {
                Text = "Statement",
                AutoSize = true,
                Location = new Point(12, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var statementBox = new ComboBox
            {
                Width = 220,
                Location = new Point(88, 14),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(40, 46, 58)
            };

            const string currentMonthFilter = "Current Month";
            var journalMonths = JournalTransactionService.GetDistinctMonths();
            statementBox.Items.AddRange(journalMonths
                .Select(month => (object)$"Balance Sheet of the {month.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}")
                .ToArray());
            const string yearStatementPrefix = "Balance Sheet of the Year ";
            statementBox.Items.AddRange(JournalTransactionService.GetDistinctYears()
                .Select(year => (object)$"{yearStatementPrefix}{year}")
                .ToArray());
            var currentMonthIndex = journalMonths.FindIndex(month => month.Year == DateTime.Now.Year && month.Month == DateTime.Now.Month);
            statementBox.SelectedIndex = statementBox.Items.Count == 0
                ? -1
                : currentMonthIndex >= 0 ? currentMonthIndex : statementBox.Items.Count - 1;

            var searchButton = new Button
            {
                Text = "Search",
                Width = 90,
                Height = 30,
                Location = new Point(350, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            searchButton.FlatAppearance.BorderSize = 0;

            var clearButton = new Button
            {
                Text = "Clear",
                Width = 80,
                Height = 30,
                Location = new Point(452, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 236, 241),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            clearButton.FlatAppearance.BorderSize = 0;

            var pdfButton = GridPdfExporter.CreateButton(544, 13);
            var grid = new DataGridView
            {
                Location = new Point(24, 135),
                Size = new Size(900, 435),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(68, 78, 94),
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 30 },
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ScrollBars = ScrollBars.None,
                Font = new Font("Segoe UI", 12F)
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                SelectionBackColor = Color.FromArgb(79, 100, 135),
                SelectionForeColor = Color.White
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(38, 46, 57),
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 6, 0),
                SelectionBackColor = Color.FromArgb(221, 232, 244),
                SelectionForeColor = Color.FromArgb(38, 46, 57)
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(38, 46, 57),
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 6, 0)
            };

            grid.Columns.Add("EntryDate", "Entry Date");
            grid.Columns.Add("JournalId", "Journal ID");
            grid.Columns.Add("Particulars", "Particulars");
            grid.Columns.Add("Dr(rs)", "Dr(rs)");
            grid.Columns.Add("Cr(rs)", "Cr(rs)");
            grid.Columns[0].FillWeight = 18;
            grid.Columns[1].FillWeight = 14;
            grid.Columns[2].FillWeight = 34;
            grid.Columns[3].FillWeight = 17;
            grid.Columns[4].FillWeight = 17;
            grid.Columns[0].Visible = false;
            grid.Columns[1].Visible = false;
            grid.Columns[0].DefaultCellStyle.Format = "dd-MMM-yyyy";
            grid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            void UpdateSheetHeader()
            {
                if (statementBox.SelectedItem is string selectedStatement
                    && !string.Equals(selectedStatement, currentMonthFilter, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(selectedStatement, "All statements", StringComparison.OrdinalIgnoreCase))
                {
                    sheetTitle.Text = selectedStatement;
                }
                else
                {
                    sheetTitle.Text = $"Balance Sheet of the {now.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}";
                }
            }

            void LoadGridData(string? statementFilter = null)
            {
                grid.Rows.Clear();

                var isCurrentMonth = string.Equals(statementFilter, currentMonthFilter, StringComparison.OrdinalIgnoreCase);
                var allRows = isCurrentMonth || (!string.IsNullOrWhiteSpace(statementFilter)
                    && !string.Equals(statementFilter, "All statements", StringComparison.OrdinalIgnoreCase))
                    ? BalanceSheetService.GetAll()
                    : null;
                DateTime? selectedMonth = isCurrentMonth
                    ? new DateTime(now.Year, now.Month, 1)
                    : null;

                if (!selectedMonth.HasValue && allRows is not null && !string.IsNullOrWhiteSpace(statementFilter))
                {
                    var statementDate = allRows
                        .Where(x => string.Equals(x.StatementTitle, statementFilter, StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.EntryDate)
                        .FirstOrDefault();
                    if (statementDate.HasValue)
                    {
                        selectedMonth = new DateTime(statementDate.Value.Year, statementDate.Value.Month, 1);
                    }
                }

                if (!selectedMonth.HasValue
                    && !isCurrentMonth
                    && !string.IsNullOrWhiteSpace(statementFilter)
                    && !string.Equals(statementFilter, "All statements", StringComparison.OrdinalIgnoreCase))
                {
                    const string statementPrefix = "Balance Sheet of the ";
                    var titleDate = statementFilter.StartsWith(statementPrefix, StringComparison.OrdinalIgnoreCase)
                        ? statementFilter.Substring(statementPrefix.Length)
                        : statementFilter;
                    if (DateTime.TryParseExact(
                        titleDate,
                        "MMMM yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedMonth))
                    {
                        selectedMonth = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                    }
                }

                List<BalanceSheetItems> rows;
                if (statementFilter is not null
                    && statementFilter.StartsWith(yearStatementPrefix, StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(statementFilter.Substring(yearStatementPrefix.Length), out var selectedYear))
                {
                    rows = JournalTransactionService.GetYearlyBalanceSheet(selectedYear);
                }
                else if (selectedMonth.HasValue)
                {
                    rows = JournalTransactionService.GetMonthlyBalanceSheet(selectedMonth.Value);
                }
                else
                {
                    rows = (allRows ?? BalanceSheetService.GetAll(statementFilter)).OrderBy(x => x.SourceRow).ToList();
                }

                foreach (var row in rows)
                {
                    var rowIndex = grid.Rows.Add(row.EntryDate, row.JournalId?.ToString() ?? string.Empty, row.Particulars, FormatAmount(row.Debit), FormatAmount(row.Credit));

                    var isSummary = row.LineType.Equals("Total", StringComparison.OrdinalIgnoreCase)
                        || row.LineType.Equals("Grand Total", StringComparison.OrdinalIgnoreCase)
                        || row.LineType.Equals("Opening Balance", StringComparison.OrdinalIgnoreCase)
                        || row.LineType.Equals("Closing Balance", StringComparison.OrdinalIgnoreCase);

                    if (isSummary)
                    {
                        grid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                        grid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(20, 29, 38);
                    }

                    grid.Rows[rowIndex].Cells[3].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    grid.Rows[rowIndex].Cells[4].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    grid.Rows[rowIndex].Cells[3].Style.ForeColor = Color.FromArgb(18, 60, 98);
                    grid.Rows[rowIndex].Cells[4].Style.ForeColor = Color.FromArgb(18, 60, 98);
                }

                pdfButton.Enabled = grid.Rows.Count > 0;
            }

            pdfButton.Click += (_, _) => GridPdfExporter.Export(this, grid, sheetTitle.Text);
            statementBox.SelectedIndexChanged += (_, _) => UpdateSheetHeader();
            searchButton.Click += (_, _) =>
            {
                var selectedStatement = statementBox.SelectedItem?.ToString();
                LoadGridData(selectedStatement == "All statements" ? null : selectedStatement);
                UpdateSheetHeader();
            };
            clearButton.Click += (_, _) =>
            {
                statementBox.SelectedIndex = statementBox.Items.Count == 0
                    ? -1
                    : currentMonthIndex >= 0 ? currentMonthIndex : statementBox.Items.Count - 1;
                LoadGridData(statementBox.SelectedItem?.ToString());
                UpdateSheetHeader();
            };

            LoadGridData(statementBox.SelectedItem?.ToString());
            UpdateSheetHeader();

            searchPanel.Controls.Add(statementLabel);
            searchPanel.Controls.Add(statementBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(clearButton);
            searchPanel.Controls.Add(pdfButton);

            Controls.Add(sheetTitle);
            Controls.Add(searchPanel);
            Controls.Add(grid);

            Resize += (_, _) =>
            {
                var width = Math.Max(0, ClientSize.Width - 48);
                searchPanel.Width = width;
                grid.Width = width;
            };
        }

        private static string FormatAmount(decimal amount)
        {
            if (amount == 0m)
            {
                return string.Empty;
            }

            return amount.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-US"));
        }
    }
}
