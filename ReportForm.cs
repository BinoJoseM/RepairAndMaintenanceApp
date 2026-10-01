using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public class ReportForm : Form
    {
        private readonly ComboBox reportTypeBox;
        private readonly DateTimePicker fromDateBox;
        private readonly DateTimePicker toDateBox;
        private readonly DataGridView grid;
        private readonly PrintDocument printDocument;

        public ReportForm()
        {
            Text = "Reports";
            Width = 980;
            Height = 680;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 248, 251);
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "Financial Reports",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20)
            };

            var filterPanel = new Panel
            {
                Location = new Point(24, 62),
                Size = new Size(900, 105),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            AddLabel(filterPanel, "Report", 14, 12);
            reportTypeBox = new ComboBox
            {
                Location = new Point(14, 34),
                Width = 205,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            reportTypeBox.Items.AddRange(new object[]
            {
                "Income Summary",
                "Expense Summary",
                "Journal Activity",
                "Category Summary"
            });
            reportTypeBox.SelectedIndex = 0;

            AddLabel(filterPanel, "From", 245, 12);
            fromDateBox = CreateDatePicker(245, 34);
            fromDateBox.Value = new DateTime(2026, 8, 1);

            AddLabel(filterPanel, "To", 405, 12);
            toDateBox = CreateDatePicker(405, 34);
            toDateBox.Value = new DateTime(2026, 8, 31);

            var generateButton = CreateButton("Generate", 575, 32, 105, Color.FromArgb(32, 74, 140), Color.White);
            var refreshButton = CreateButton("Refresh", 695, 32, 90, Color.FromArgb(232, 236, 240), Color.FromArgb(60, 72, 84));
            var exportExcelButton = CreateButton("Export Excel", 575, 67, 105, Color.FromArgb(70, 120, 75), Color.White);
            var exportPdfButton = CreateButton("Export PDF", 695, 67, 90, Color.FromArgb(180, 55, 55), Color.White);
            generateButton.Click += (_, _) => GenerateReport();
            refreshButton.Click += (_, _) => GenerateReport();
            exportExcelButton.Click += (_, _) => ExportExcel();
            exportPdfButton.Click += (_, _) => ExportPdf();

            filterPanel.Controls.Add(reportTypeBox);
            filterPanel.Controls.Add(fromDateBox);
            filterPanel.Controls.Add(toDateBox);
            filterPanel.Controls.Add(generateButton);
            filterPanel.Controls.Add(refreshButton);
            filterPanel.Controls.Add(exportExcelButton);
            filterPanel.Controls.Add(exportPdfButton);

            grid = new DataGridView
            {
                Location = new Point(24, 180),
                Size = new Size(900, 412),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            Controls.Add(title);
            Controls.Add(filterPanel);
            Controls.Add(grid);

            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            GenerateReport();
        }

        private void ExportExcel()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Excel CSV file (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"{reportTypeBox.Text.Replace(" ", "_")}_{fromDateBox.Value:yyyyMMdd}_{toDateBox.Value:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", GetGridHeaders()));
            foreach (DataGridViewRow row in grid.Rows)
            {
                var values = new string[grid.Columns.Count];
                for (var columnIndex = 0; columnIndex < grid.Columns.Count; columnIndex++)
                {
                    values[columnIndex] = EscapeCsv(row.Cells[columnIndex].Value?.ToString() ?? string.Empty);
                }

                builder.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
            MessageBox.Show("Report exported for Excel.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportPdf()
        {
            var pdfPrinter = string.Empty;
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printer.Contains("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase))
                {
                    pdfPrinter = printer;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pdfPrinter))
            {
                printDocument!.PrinterSettings.PrinterName = pdfPrinter;
            }

            using var dialog = new PrintDialog
            {
                Document = printDocument!,
                UseEXDialog = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                printDocument!.Print();
            }
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var bodyFont = new Font("Segoe UI", 9F);
            var y = e.MarginBounds.Top;
            e.Graphics!.DrawString(reportTypeBox.Text, titleFont, Brushes.Black, e.MarginBounds.Left, y);
            y += 32;
            e.Graphics!.DrawString($"Period: {fromDateBox.Value:yyyy-MM-dd} to {toDateBox.Value:yyyy-MM-dd}", bodyFont, Brushes.Black, e.MarginBounds.Left, y);
            y += 26;

            var headers = GetGridHeaders();
            e.Graphics!.DrawString(string.Join("    ", headers), bodyFont, Brushes.Black, e.MarginBounds.Left, y);
            y += 22;

            foreach (DataGridViewRow row in grid.Rows)
            {
                var values = new string[grid.Columns.Count];
                for (var columnIndex = 0; columnIndex < grid.Columns.Count; columnIndex++)
                {
                    values[columnIndex] = row.Cells[columnIndex].Value?.ToString() ?? string.Empty;
                }

                e.Graphics!.DrawString(string.Join("    ", values), bodyFont, Brushes.Black, e.MarginBounds.Left, y);
                y += 20;
                if (y > e.MarginBounds.Bottom - 20)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
        }

        private string[] GetGridHeaders()
        {
            var headers = new string[grid.Columns.Count];
            for (var columnIndex = 0; columnIndex < grid.Columns.Count; columnIndex++)
            {
                headers[columnIndex] = EscapeCsv(grid.Columns[columnIndex].HeaderText);
            }

            return headers;
        }

        private static string EscapeCsv(string value)
        {
            return value.Contains(',') || value.Contains('"') || value.Contains('\n')
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }

        private void GenerateReport()
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            switch (reportTypeBox.SelectedItem?.ToString())
            {
                case "Income Summary":
                    grid.Columns.Add("Category", "Category");
                    grid.Columns.Add("Transactions", "Transactions");
                    grid.Columns.Add("Total", "Total");
                    grid.Rows.Add("Service Revenue", "3", "$9,400.00");
                    grid.Rows.Add("Contract Revenue", "1", "$3,280.00");
                    grid.Rows.Add("Consulting", "1", "$1,450.00");
                    break;

                case "Expense Summary":
                    grid.Columns.Add("Category", "Category");
                    grid.Columns.Add("Transactions", "Transactions");
                    grid.Columns.Add("Total", "Total");
                    grid.Rows.Add("Utilities", "1", "$750.00");
                    grid.Rows.Add("Office", "1", "$420.00");
                    grid.Rows.Add("Maintenance", "1", "$1,930.00");
                    grid.Rows.Add("Insurance", "1", "$1,280.00");
                    grid.Rows.Add("Travel", "1", "$610.00");
                    break;

                case "Journal Activity":
                    grid.Columns.Add("Account", "Account");
                    grid.Columns.Add("Debits", "Debits");
                    grid.Columns.Add("Credits", "Credits");
                    grid.Rows.Add("Cash", "$6,200.00", "$0.00");
                    grid.Rows.Add("Office Supplies", "$420.00", "$0.00");
                    grid.Rows.Add("Accounts Receivable", "$0.00", "$1,450.00");
                    grid.Rows.Add("Maintenance Expense", "$1,930.00", "$0.00");
                    grid.Rows.Add("Bank", "$0.00", "$2,100.00");
                    break;

                default:
                    grid.Columns.Add("Category", "Category");
                    grid.Columns.Add("Type", "Type");
                    grid.Columns.Add("Status", "Status");
                    grid.Rows.Add("Service Revenue", "Income", "Active");
                    grid.Rows.Add("Contract Revenue", "Income", "Active");
                    grid.Rows.Add("Utilities", "Expense", "Active");
                    grid.Rows.Add("Maintenance", "Expense", "Active");
                    break;
            }
        }

        private static DateTimePicker CreateDatePicker(int x, int y)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Width = 135,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd"
            };
        }

        private static void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 72, 84)
            });
        }

        private static Button CreateButton(string text, int x, int y, int width, Color backColor, Color foreColor)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
