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

            var statementItems = new List<string> { "All statements" };
            statementItems.AddRange(BalanceSheetService.GetAll().Select(x => x.StatementTitle).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x));
            statementBox.Items.AddRange(statementItems.Distinct().ToArray());
            statementBox.SelectedIndex = 0;

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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
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

            grid.Columns.Add("Particulars", "Particulars");
            grid.Columns.Add("Dr(rs)", "Dr(rs)");
            grid.Columns.Add("Cr(rs)", "Cr(rs)");
            grid.Columns[0].Width = 510;
            grid.Columns[1].Width = 180;
            grid.Columns[2].Width = 180;
            grid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            void UpdateSheetHeader()
            {
                if (statementBox.SelectedItem is string selectedStatement && !string.Equals(selectedStatement, "All statements", StringComparison.OrdinalIgnoreCase))
                {
                    sheetTitle.Text = selectedStatement + " of the " + now.ToString("MMMM yyyy");
                }
                else
                {
                    sheetTitle.Text = "Balance Sheet of the " + now.ToString("MMMM yyyy");
                }
            }

            void LoadGridData(string? statementFilter = null)
            {
                grid.Rows.Clear();

                var rows = BalanceSheetService.GetAll(statementFilter);
                foreach (var row in rows.OrderBy(x => x.SourceRow))
                {
                    var rowIndex = grid.Rows.Add(row.Particulars, FormatAmount(row.Debit), FormatAmount(row.Credit));

                    var isSummary = row.LineType.Equals("Total", StringComparison.OrdinalIgnoreCase)
                        || row.LineType.Equals("Grand Total", StringComparison.OrdinalIgnoreCase)
                        || row.LineType.Equals("Opening Balance", StringComparison.OrdinalIgnoreCase)
                        || row.LineType.Equals("Closing Balance", StringComparison.OrdinalIgnoreCase);

                    if (isSummary)
                    {
                        grid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                        grid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(20, 29, 38);
                    }

                    grid.Rows[rowIndex].Cells[1].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    grid.Rows[rowIndex].Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    grid.Rows[rowIndex].Cells[1].Style.ForeColor = Color.FromArgb(18, 60, 98);
                    grid.Rows[rowIndex].Cells[2].Style.ForeColor = Color.FromArgb(18, 60, 98);
                }
            }

            statementBox.SelectedIndexChanged += (_, _) => UpdateSheetHeader();
            searchButton.Click += (_, _) =>
            {
                var selectedStatement = statementBox.SelectedItem?.ToString();
                LoadGridData(selectedStatement == "All statements" ? null : selectedStatement);
                UpdateSheetHeader();
            };
            clearButton.Click += (_, _) =>
            {
                statementBox.SelectedIndex = 0;
                LoadGridData();
                UpdateSheetHeader();
            };

            LoadGridData();
            UpdateSheetHeader();

            searchPanel.Controls.Add(statementLabel);
            searchPanel.Controls.Add(statementBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(clearButton);

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
