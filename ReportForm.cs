using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RepairAndMaintenanceApp.Entities;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class ReportForm : Form
    {
        private readonly ComboBox reportTypeBox;
        private readonly DateTimePicker fromDateBox;
        private readonly DateTimePicker toDateBox;
        private readonly DataGridView grid;
        private readonly PrintDocument printDocument;
        private int printRowIndex;
        private int printPageNumber;

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
            reportTypeBox.Items.AddRange(ReportService.ReportNames);
            reportTypeBox.SelectedIndex = 0;

            AddLabel(filterPanel, "From", 245, 12);
            fromDateBox = CreateDatePicker(245, 34);
            fromDateBox.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            AddLabel(filterPanel, "To", 405, 12);
            toDateBox = CreateDatePicker(405, 34);
            toDateBox.Value = fromDateBox.Value.AddMonths(1).AddDays(-1);

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
            printDocument.DefaultPageSettings.Landscape = true;
            printDocument.BeginPrint += PrintDocument_BeginPrint;
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
            AppMessageBox.Show("Report exported for Excel.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void PrintDocument_BeginPrint(object? sender, PrintEventArgs e)
        {
            printRowIndex = 0;
            printPageNumber = 0;
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            var graphics = e.Graphics!;
            var bounds = e.MarginBounds;
            printPageNumber++;

            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var subtitleFont = new Font("Segoe UI", 9F);
            using var headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var bodyFont = new Font("Segoe UI", 9F);
            using var totalFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var headerBrush = new SolidBrush(Color.FromArgb(79, 100, 135));
            using var totalBrush = new SolidBrush(Color.FromArgb(232, 236, 240));
            using var gridPen = new Pen(Color.FromArgb(190, 197, 207));

            float y = bounds.Top;
            graphics.DrawString(reportTypeBox.Text, titleFont, Brushes.Black, bounds.Left, y);
            y += 32;
            graphics.DrawString($"Period: {fromDateBox.Value:dd-MMM-yyyy} to {toDateBox.Value:dd-MMM-yyyy}", subtitleFont, Brushes.Black, bounds.Left, y);
            y += 28;

            var columnCount = grid.Columns.Count;
            if (columnCount == 0)
            {
                e.HasMorePages = false;
                return;
            }

            // Column widths follow the on-screen grid, scaled to the printable width.
            var gridWidth = 0;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                gridWidth += Math.Max(column.Width, 1);
            }

            var widths = new float[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                widths[i] = Math.Max(grid.Columns[i].Width, 1) * (float)bounds.Width / gridWidth;
            }

            const float rowHeight = 24F;
            const float padding = 5F;

            void DrawRow(string[] values, Font font, Brush textBrush, Brush? fill, float top)
            {
                var x = (float)bounds.Left;
                for (var i = 0; i < columnCount; i++)
                {
                    var cell = new RectangleF(x, top, widths[i], rowHeight);
                    if (fill != null)
                    {
                        graphics.FillRectangle(fill, cell);
                    }

                    graphics.DrawRectangle(gridPen, cell.X, cell.Y, cell.Width, cell.Height);
                    using var format = new StringFormat
                    {
                        Alignment = grid.Columns[i].DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleRight ? StringAlignment.Far : StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    var textBounds = new RectangleF(cell.X + padding, cell.Y, cell.Width - (2 * padding), cell.Height);
                    graphics.DrawString(values[i], font, textBrush, textBounds, format);
                    x += widths[i];
                }
            }

            var headers = new string[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                headers[i] = grid.Columns[i].HeaderText;
            }

            DrawRow(headers, headerFont, Brushes.White, headerBrush, y);
            y += rowHeight;

            while (printRowIndex < grid.Rows.Count)
            {
                if (y + rowHeight > bounds.Bottom - 20)
                {
                    e.HasMorePages = true;
                    graphics.DrawString($"Page {printPageNumber}", subtitleFont, Brushes.Gray, bounds.Right - 50, bounds.Bottom);
                    return;
                }

                var row = grid.Rows[printRowIndex];
                var values = new string[columnCount];
                for (var i = 0; i < columnCount; i++)
                {
                    values[i] = row.Cells[i].Value?.ToString() ?? string.Empty;
                }

                var isTotal = row.DefaultCellStyle.Font?.Bold == true;
                DrawRow(values, isTotal ? totalFont : bodyFont, Brushes.Black, isTotal ? totalBrush : null, y);
                y += rowHeight;
                printRowIndex++;
            }

            graphics.DrawString($"Page {printPageNumber}", subtitleFont, Brushes.Gray, bounds.Right - 50, bounds.Bottom);
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

            ReportTable report;
            try
            {
                report = ReportService.Generate(reportTypeBox.SelectedItem?.ToString() ?? string.Empty, fromDateBox.Value, toDateBox.Value);
            }
            catch (ArgumentException exception)
            {
                AppMessageBox.Show(exception.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (var column = 0; column < report.Headers.Count; column++)
            {
                var index = grid.Columns.Add($"Column{column}", report.Headers[column]);
                if (report.NumericColumns.Contains(column))
                {
                    grid.Columns[index].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    grid.Columns[index].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            for (var rowIndex = 0; rowIndex < report.Rows.Count; rowIndex++)
            {
                var gridRow = grid.Rows.Add(report.Rows[rowIndex]);
                if (report.TotalRows.Contains(rowIndex))
                {
                    grid.Rows[gridRow].DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    grid.Rows[gridRow].DefaultCellStyle.BackColor = Color.FromArgb(232, 236, 240);
                }
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
