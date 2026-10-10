using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    /// <summary>Prints a DataGridView as a bordered table, defaulting to the "Microsoft Print to PDF" printer.</summary>
    public static class GridPdfExporter
    {
        public static Button CreateButton(int x, int y)
        {
            var button = new Button
            {
                Text = "Generate PDF",
                Width = 110,
                Height = 30,
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 55, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        public static void Export(IWin32Window owner, DataGridView grid, string title, string? subtitle = null)
        {
            var columns = grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();
            if (columns.Count == 0 || grid.Rows.Count == 0)
            {
                return;
            }

            using var document = new PrintDocument();
            document.DefaultPageSettings.Landscape = true;
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printer.Contains("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase))
                {
                    document.PrinterSettings.PrinterName = printer;
                    break;
                }
            }

            var rowIndex = 0;
            var pageNumber = 0;
            document.BeginPrint += (_, _) =>
            {
                rowIndex = 0;
                pageNumber = 0;
            };
            document.PrintPage += (_, e) =>
            {
                pageNumber++;
                DrawPage(e, grid, columns, title, subtitle, ref rowIndex, pageNumber);
            };

            using var dialog = new PrintDialog { Document = document, UseEXDialog = true };
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                document.Print();
            }
        }

        private static void DrawPage(PrintPageEventArgs e, DataGridView grid, List<DataGridViewColumn> columns, string title, string? subtitle, ref int rowIndex, int pageNumber)
        {
            var graphics = e.Graphics!;
            var bounds = e.MarginBounds;

            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var subtitleFont = new Font("Segoe UI", 9F);
            using var headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var bodyFont = new Font("Segoe UI", 9F);
            using var boldFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var headerBrush = new SolidBrush(Color.FromArgb(79, 100, 135));
            using var totalBrush = new SolidBrush(Color.FromArgb(232, 236, 240));
            using var gridPen = new Pen(Color.FromArgb(190, 197, 207));

            float y = bounds.Top;
            graphics.DrawString(title, titleFont, Brushes.Black, bounds.Left, y);
            y += 32;
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                graphics.DrawString(subtitle, subtitleFont, Brushes.Black, bounds.Left, y);
                y += 24;
            }

            y += 4;

            var gridWidth = columns.Sum(c => Math.Max(c.Width, 1));
            var widths = columns.Select(c => Math.Max(c.Width, 1) * (float)bounds.Width / gridWidth).ToArray();
            const float rowHeight = 24F;
            const float padding = 5F;

            void DrawRow(string[] values, StringAlignment[] alignments, Font font, Brush textBrush, Brush? fill, float top)
            {
                var x = (float)bounds.Left;
                for (var i = 0; i < columns.Count; i++)
                {
                    var cell = new RectangleF(x, top, widths[i], rowHeight);
                    if (fill != null)
                    {
                        graphics.FillRectangle(fill, cell);
                    }

                    graphics.DrawRectangle(gridPen, Rectangle.Round(cell));
                    using var format = new StringFormat
                    {
                        Alignment = alignments[i],
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    var textBounds = new RectangleF(cell.X + padding, cell.Y, cell.Width - (2 * padding), cell.Height);
                    graphics.DrawString(values[i], font, textBrush, textBounds, format);
                    x += widths[i];
                }
            }

            var headers = columns.Select(c => c.HeaderText).ToArray();
            var headerAlignments = columns.Select(_ => StringAlignment.Near).ToArray();
            DrawRow(headers, headerAlignments, headerFont, Brushes.White, headerBrush, y);
            y += rowHeight;

            while (rowIndex < grid.Rows.Count)
            {
                if (y + rowHeight > bounds.Bottom - 20)
                {
                    graphics.DrawString($"Page {pageNumber}", subtitleFont, Brushes.Gray, bounds.Right - 50, bounds.Bottom);
                    e.HasMorePages = true;
                    return;
                }

                var row = grid.Rows[rowIndex];
                var values = new string[columns.Count];
                var alignments = new StringAlignment[columns.Count];
                for (var i = 0; i < columns.Count; i++)
                {
                    var cell = row.Cells[columns[i].Index];
                    values[i] = cell.FormattedValue?.ToString() ?? string.Empty;
                    var alignment = cell.InheritedStyle.Alignment;
                    alignments[i] = alignment is DataGridViewContentAlignment.MiddleRight
                        or DataGridViewContentAlignment.TopRight
                        or DataGridViewContentAlignment.BottomRight
                        ? StringAlignment.Far
                        : StringAlignment.Near;
                }

                var isBold = row.Cells[columns[0].Index].InheritedStyle.Font?.Bold == true;
                DrawRow(values, alignments, isBold ? boldFont : bodyFont, Brushes.Black, isBold ? totalBrush : null, y);
                y += rowHeight;
                rowIndex++;
            }

            graphics.DrawString($"Page {pageNumber}", subtitleFont, Brushes.Gray, bounds.Right - 50, bounds.Bottom);
            e.HasMorePages = false;
        }
    }
}
