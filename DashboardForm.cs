using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RepairAndMaintenanceApp.DataLayer;

namespace RepairAndMaintenanceApp
{
    public class DashboardForm : Form
    {
        public DashboardForm()
        {
            Text = "Dashboard";
            Width = 1280;
            Height = 900;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            MinimumSize = new Size(1100, 780);
            Font = new Font("Segoe UI", 10F);

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 249)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(79, 100, 135)
            };

            var headerContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 20, 0),
                ColumnCount = 3,
                RowCount = 1
            };
            headerContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 520F));
            headerContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));

            var brand = new Label
            {
                Text = "Repair & Maintenance App",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                UseMnemonic = false,
                AutoSize = false,
                Size = new Size(440, 36),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var brandMark = new PictureBox
            {
                Image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "white-Home.png")),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 2, 8, 2)
            };

            var brandPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 18, 0, 18),
                Margin = Padding.Empty
            };
            brandPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            brandPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            brandPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            brandPanel.Controls.Add(brandMark, 0, 0);
            brandPanel.Controls.Add(brand, 1, 0);

            headerContent.Controls.Add(brandPanel, 0, 0);

            // var searchBox = new TextBox
            // {
            //     Width = 260,
            //     Height = 32,
            //     Anchor = AnchorStyles.Left,
            //     Font = new Font("Segoe UI", 11F),
            //     Text = " Search projects",
            //     ForeColor = Color.FromArgb(140, 129, 154),
            //     BorderStyle = BorderStyle.None,
            //     BackColor = Color.FromArgb(196, 160, 232)
            // };
            // searchBox.Multiline = false;
            // var searchWrapper = new Panel
            // {
            //     Width = 290,
            //     Height = 34,
            //     BackColor = Color.FromArgb(130, 145, 168),
            //     BorderStyle = BorderStyle.None,
            //     Padding = new Padding(10, 4, 10, 4),
            //     Anchor = AnchorStyles.Left
            // };
            // searchWrapper.Controls.Add(searchBox);
            // headerContent.Controls.Add(searchWrapper, 1, 0);

            var userArea = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 18, 0, 18),
                Anchor = AnchorStyles.Right,
                Margin = Padding.Empty
            };

            var logoutButton = new Button
            {
                Text = "⇥  Log out",
                Size = new Size(112, 36),
                AutoSize = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Cursor = Cursors.Hand
            };
            logoutButton.FlatAppearance.BorderSize = 0;
            logoutButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(68, 85, 120);
            logoutButton.Click += (_, _) =>
            {
                new LoginForm().Show();
                Close();
            };
            userArea.Controls.Add(logoutButton);

            headerContent.Controls.Add(userArea, 2, 0);
            header.Controls.Add(headerContent);

            var navigation = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(91, 109, 146)
            };

            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            Panel content = null!;
            TableLayoutPanel dashboardLayout = null!;

            var navItems = new[]
            {
                new { Text = "Home", FormType = typeof(DashboardForm) },
                new { Text = "Journal", FormType = typeof(JournalForm) },
                new { Text = "Expenses", FormType = typeof(LedgerExpensesForm) },
                new { Text = "Income", FormType = typeof(LedgerIncomeForm) },
                new { Text = "Categories", FormType = typeof(CategoryMasterForm) },
                new { Text = "Particulars", FormType = typeof(ParticularMasterForm) },
                new {Text = "Balance Sheet", FormType = typeof(BalanceSheetForm) },
                new { Text = "Monthly Summary", FormType = typeof(MonthlyIncomeExpenseSummaryForm) },
                new { Text = "Reports", FormType = typeof(ReportForm) }               
            };

            foreach (var item in navItems)
            {
                var navButton = new Button
                {
                    Text = item.Text,
                    FlatStyle = FlatStyle.Flat,
                    AutoSize = true,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                    BackColor = Color.FromArgb(91, 109, 146),
                    Margin = new Padding(4, 0, 18, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(8, 6, 8, 6)
                };
                navButton.FlatAppearance.BorderSize = 0;
                navButton.Cursor = Cursors.Hand;

                if (item.Text == "Home")
                {
                    navButton.BackColor = Color.FromArgb(68, 85, 120);
                    navButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                }

                navButton.Click += (_, _) =>
                {
                    foreach (Button button in navFlow.Controls)
                    {
                        button.BackColor = Color.FromArgb(91, 109, 146);
                        button.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
                    }

                    navButton.BackColor = Color.FromArgb(68, 85, 120);
                    navButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

                    if (item.FormType == typeof(DashboardForm))
                    {
                        ShowContentArea(content, dashboardLayout);
                        return;
                    }

                    var form = (Form)Activator.CreateInstance(item.FormType)!;
                    form.TopLevel = false;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Dock = DockStyle.Fill;
                    form.Visible = true;
                    ShowContentArea(content, form);
                    form.Show();
                };
                navFlow.Controls.Add(navButton);
            }
            navigation.Controls.Add(navFlow);

            content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 16),
                BackColor = Color.FromArgb(244, 246, 249)
            };

            var welcomeRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(244, 246, 249)
            };

            var welcomeLabel = new Label
            {
                Text = "Hi,Welcome Back!",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 46, 58),
                AutoSize = true,
                Location = new Point(8, 20)
            };

            var subLabel = new Label
            {
                Text = "Your erp admin template",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(109, 120, 134),
                AutoSize = true,
                Location = new Point(210, 28)
            };

            var dateLabel = new Label
            {
                Text = "Saturday, 17 Jun 2020",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(128, 118, 138),
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Location = new Point(850, 28)
            };
            welcomeRow.Controls.Add(dateLabel);
            //welcomeRow.Controls.Add(subLabel);
            welcomeRow.Controls.Add(welcomeLabel);

                // var messagePanel = new Panel
                // {
                //     Dock = DockStyle.Top,
                //     Height = 142,
                //     Margin = new Padding(0, 0, 0, 20),
                //     BackColor = Color.FromArgb(223, 230, 238),
                //     BorderStyle = BorderStyle.None
                // };

            // var messageText = new Label
            // {
            //     Text = "Discuss what you are doing and how we can help.",
            //     Font = new Font("Segoe UI", 16F),
            //     ForeColor = Color.FromArgb(39, 48, 61),
            //     AutoSize = true,
            //     Location = new Point(28, 32)
            // };
            // var overviewButton = new Button
            // {
            //     Text = "overview",
            //     Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            //     ForeColor = Color.White,
            //     BackColor = Color.FromArgb(70, 97, 146),
            //     FlatStyle = FlatStyle.Flat,
            //     Size = new Size(118, 35),
            //     Location = new Point(430, 28)
            // };
            // overviewButton.FlatAppearance.BorderSize = 0;
            // var deskIllustration = new Panel
            // {
            //     Size = new Size(230, 110),
            //     Location = new Point(760, 14),
            //     BackColor = Color.FromArgb(223, 230, 238)
            // };
            // deskIllustration.Paint += (sender, e) =>
            // {
            //     e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 169, 174)), new Rectangle(140, 40, 40, 25));
            //     e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(84, 133, 68)), new Rectangle(155, 65, 15, 20));
            //     e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(236, 198, 113)), new Rectangle(138, 16, 38, 38));
            //     e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(81, 109, 126)), new Rectangle(120, 55, 90, 8));
            //     e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(30, 40, 54)), new Rectangle(150, 60, 10, 35));
            //     e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(243, 222, 138)), new Rectangle(90, 80, 130, 10));
            //     e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(10, 79, 110)), new Rectangle(90, 90, 130, 10));
            // };

            // // messagePanel.Controls.Add(messageText);
            // messagePanel.Controls.Add(overviewButton);
            // messagePanel.Controls.Add(deskIllustration);

            var statsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 240,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(244, 246, 249),
                Margin = new Padding(0, 0, 0, 24)
            };
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var totalCredits = IncomeLedgerDataAccess.GetAll().Sum(entry => entry.Amount);
            var expenseEntries = ExpenseLedgerDataAccess.GetAll();
            var totalDebits = expenseEntries.Sum(entry => entry.Amount);
            var incomeEntries = IncomeLedgerDataAccess.GetAll();
            var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var currentMonthName = currentMonthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            decimal SumForMonth(IEnumerable<(DateTime? EntryDate, string SourceFile, decimal Amount)> entries, DateTime monthStart)
            {
                var nextMonthStart = monthStart.AddMonths(1);
                var monthName = monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                return entries
                    .Where(entry =>
                        (entry.EntryDate.HasValue
                            && entry.EntryDate.Value >= monthStart
                            && entry.EntryDate.Value < nextMonthStart)
                        || (!entry.EntryDate.HasValue
                            && entry.SourceFile.Contains(monthName, StringComparison.OrdinalIgnoreCase)))
                    .Sum(entry => entry.Amount);
            }

            var incomeMonthEntries = incomeEntries.Select(entry => (entry.EntryDate, entry.SourceFile, entry.Amount));
            var expenseMonthEntries = expenseEntries.Select(entry => (entry.EntryDate, entry.SourceFile, entry.Amount));
            var currentMonthIncomeTotal = SumForMonth(incomeMonthEntries, currentMonthStart);
            var currentMonthExpenseTotal = SumForMonth(expenseMonthEntries, currentMonthStart);
            var currencyFormat = CultureInfo.GetCultureInfo("en-IN");
            var incomeVsExpenses = CreateMetricCard(
                "Income vs. Expenses",
                $"₹{totalCredits.ToString("#,##0.00", currencyFormat)}",
                $"₹{totalDebits.ToString("#,##0.00", currencyFormat)}",
                "Income",
                "Expenses",
                Color.FromArgb(68, 101, 128),
                Color.FromArgb(207, 183, 122),
                true,
                $"₹{(totalCredits - totalDebits).ToString("#,##0.00", currencyFormat)}",
                "Balance");
            var (totalPayables, totalOverdue) = GetPayableTotals();
            var payables = CreateMetricCard(
                "Total Payables",
                $"₹{totalPayables.ToString("#,##0.00", currencyFormat)}",
                $"₹{totalOverdue.ToString("#,##0.00", currencyFormat)}",
                "Total Payables",
                "Overdue",
                Color.FromArgb(54, 107, 125),
                Color.FromArgb(207, 183, 122),
                false,
                $"₹{(totalPayables - totalOverdue).ToString("#,##0.00", currencyFormat)}",
                "Balance to Pay");
            payables.Margin = Padding.Empty;
            statsLayout.Controls.Add(incomeVsExpenses, 0, 0);
            statsLayout.Controls.Add(payables, 1, 0);

            var bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 240,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.FromArgb(244, 246, 249),
                Margin = new Padding(0, 0, 0, 12)
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var (expenseCard, expenseValueLabel, expenseCaptionLabel) = CreateSmallLedgerCard(
                $"₹{currentMonthExpenseTotal.ToString("#,##0.00", currencyFormat)}",
                $"{currentMonthName} Expenses",
                Color.FromArgb(170, 80, 72),
                "↘",
                Color.FromArgb(250, 236, 234),
                Color.FromArgb(214, 123, 112));
            var (budgetCard, incomeValueLabel, incomeCaptionLabel) = CreateSmallLedgerCard(
                $"₹{currentMonthIncomeTotal.ToString("#,##0.00", currencyFormat)}",
                $"{currentMonthName} Income",
                Color.FromArgb(220, 199, 130),
                "◌",
                Color.FromArgb(248, 244, 227),
                Color.FromArgb(224, 188, 77));
            bottomRow.Controls.Add(expenseCard, 0, 0);
            bottomRow.Controls.Add(budgetCard, 1, 0);

            var monthSelector = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 249),
                Margin = new Padding(0, 4, 0, 0)
            };
            var monthSelectorTitle = new Label
            {
                Text = "Monthly Income & Expenses",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 72, 84)
            };
            var monthCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = Padding.Empty
            };
            var monthButtons = new List<Button>();

            void SelectMonth(DateTime monthStart)
            {
                var monthName = monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                expenseValueLabel.Text = $"₹{SumForMonth(expenseMonthEntries, monthStart).ToString("#,##0.00", currencyFormat)}";
                expenseCaptionLabel.Text = $"{monthName} Expenses";
                incomeValueLabel.Text = $"₹{SumForMonth(incomeMonthEntries, monthStart).ToString("#,##0.00", currencyFormat)}";
                incomeCaptionLabel.Text = $"{monthName} Income";

                foreach (var monthButton in monthButtons)
                {
                    var isSelected = monthButton.Tag is DateTime buttonMonth
                        && buttonMonth.Year == monthStart.Year
                        && buttonMonth.Month == monthStart.Month;
                    monthButton.BackColor = isSelected ? Color.FromArgb(79, 100, 135) : Color.White;
                    monthButton.ForeColor = isSelected ? Color.White : Color.FromArgb(60, 72, 84);
                }
            }

            for (var monthOffset = 0; monthOffset < 6; monthOffset++)
            {
                var monthStart = currentMonthStart.AddMonths(-monthOffset);
                var monthButton = new Button
                {
                    Text = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Tag = monthStart,
                    Width = 112,
                    Height = 34,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 72, 84),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Margin = new Padding(0, 0, 8, 0),
                    Cursor = Cursors.Hand
                };
                monthButton.FlatAppearance.BorderColor = Color.FromArgb(215, 222, 230);
                monthButton.FlatAppearance.BorderSize = 1;
                monthButton.Click += (_, _) => SelectMonth(monthStart);
                monthButtons.Add(monthButton);
                monthCards.Controls.Add(monthButton);
            }

            monthSelector.Controls.Add(monthCards);
            monthSelector.Controls.Add(monthSelectorTitle);
            SelectMonth(currentMonthStart);

            var journalHealth = CreateJournalHealthCard(() =>
            {
                foreach (Button button in navFlow.Controls)
                {
                    if (string.Equals(button.Text, "Journal", StringComparison.Ordinal))
                    {
                        button.PerformClick();
                        return;
                    }
                }
            });
            bottomRow.Controls.Add(journalHealth, 2, 0);

            dashboardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.FromArgb(244, 246, 249),
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 240F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 240F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            dashboardLayout.Controls.Add(welcomeRow, 0, 0);
            dashboardLayout.Controls.Add(statsLayout, 0, 1);
            dashboardLayout.Controls.Add(bottomRow, 0, 3);
            dashboardLayout.Controls.Add(monthSelector, 0, 4);
            content.Controls.Add(dashboardLayout);

            root.Controls.Add(content);
            root.Controls.Add(navigation);
            root.Controls.Add(header);
            Controls.Add(root);
        }

        private static void ShowContentArea(Panel host, Control content)
        {
            host.SuspendLayout();
            host.Controls.Clear();

            var wrapper = new Panel
            {
                Dock = DockStyle.None,
                Size = host.ClientSize,
                Location = new Point(24, 0),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            content.Dock = DockStyle.Fill;
            content.Visible = true;
            wrapper.Controls.Add(content);
            host.Controls.Add(wrapper);

            var fadeOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 255, 255),
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            host.Controls.Add(fadeOverlay);
            fadeOverlay.BringToFront();

            var alpha = 255;
            var slideOffset = 24;
            var fadeTimer = new System.Windows.Forms.Timer
            {
                Interval = 18
            };
            fadeTimer.Tick += (_, _) =>
            {
                alpha -= 26;
                slideOffset = Math.Max(0, slideOffset - 8);
                wrapper.Location = new Point(slideOffset, 0);

                if (alpha <= 0)
                {
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                    host.Controls.Remove(fadeOverlay);
                    fadeOverlay.Dispose();
                    return;
                }

                fadeOverlay.BackColor = Color.FromArgb(alpha, 255, 255, 255);
            };
            fadeTimer.Start();
            host.ResumeLayout();
        }

        private static Panel CreateMetricCard(
            string title,
            string currentValue,
            string overdueValue,
            string currentLabel,
            string overdueLabel,
            Color cardColor,
            Color accentColor,
            bool isReceivable,
            string? thirdValue = null,
            string? thirdLabel = null)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 180,
                BackColor = cardColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 12, 0)
            };
            if (!isReceivable)
            {
                panel.Paint += (_, e) =>
                {
                    using var backgroundBrush = new LinearGradientBrush(
                        panel.ClientRectangle,
                        Color.FromArgb(39, 83, 99),
                        Color.FromArgb(61, 127, 139),
                        LinearGradientMode.Horizontal);
                    e.Graphics.FillRectangle(backgroundBrush, panel.ClientRectangle);
                };
            }

            var titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 18)
            };

            var track = new Panel
            {
                Width = 220,
                Height = 6,
                BackColor = Color.FromArgb(255, 255, 255),
                Location = new Point(24, 60)
            };
            var progress = new Panel
            {
                Width = 150,
                Height = 6,
                BackColor = accentColor,
                Location = new Point(24, 60)
            };

            var hasThirdValue = thirdValue is not null;
            var valueCount = hasThirdValue ? 3 : 2;
            var valuesPanel = new TableLayoutPanel
            {
                Width = hasThirdValue ? 520 : 320,
                Height = 92,
                Location = new Point(24, 76),
                ColumnCount = valueCount,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            for (var column = 0; column < valueCount; column++)
            {
                valuesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / valueCount));
            }
            valuesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            valuesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

            var valueFont = new Font("Segoe UI", hasThirdValue ? 17F : 22F, FontStyle.Bold);
            var labelFont = new Font("Segoe UI", 11F);
            var secondaryTextColor = isReceivable
                ? Color.FromArgb(246, 232, 240)
                : Color.FromArgb(224, 239, 241);
            var metricValues = new[] { currentValue, overdueValue, thirdValue };
            var metricLabels = new[] { currentLabel, overdueLabel, thirdLabel };
            for (var index = 0; index < valueCount; index++)
            {
                var valueLabel = new Label
                {
                    Text = metricValues[index] ?? string.Empty,
                    AutoSize = true,
                    Font = valueFont,
                    ForeColor = Color.White,
                    Anchor = hasThirdValue || index == 0 ? AnchorStyles.Left : AnchorStyles.Right,
                    Padding = Padding.Empty
                };
                var captionLabel = new Label
                {
                    Text = metricLabels[index] ?? string.Empty,
                    AutoSize = true,
                    Font = labelFont,
                    ForeColor = secondaryTextColor,
                    Anchor = hasThirdValue || index == 0 ? AnchorStyles.Left : AnchorStyles.Right,
                    Padding = Padding.Empty
                };

                valuesPanel.Controls.Add(valueLabel, index, 0);
                valuesPanel.Controls.Add(captionLabel, index, 1);
            }

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(track);
            panel.Controls.Add(progress);
            panel.Controls.Add(valuesPanel);
            return panel;
        }

        private static (Panel Card, Label ValueLabel, Label CaptionLabel) CreateSmallLedgerCard(string value, string label, Color color, string glyph, Color? backgroundColor = null, Color? accentColor = null)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
            BackColor = backgroundColor ?? Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 12, 0),
                Padding = new Padding(18)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var mark = new Panel
            {
                Size = new Size(72, 72),
                Anchor = AnchorStyles.None,
                BackColor = Color.FromArgb(
                    (color.R + 255) / 2,
                    (color.G + 255) / 2,
                    (color.B + 255) / 2),
                Margin = Padding.Empty
            };
            var glyphLabel = new Label
            {
                Text = glyph,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 25F, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mark.Controls.Add(glyphLabel);

            var accent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = accentColor ?? Color.FromArgb(225, 229, 234),
                Margin = new Padding(6, 16, 6, 16)
            };

            var textLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = new Padding(4, 0, 0, 0)
            };
            textLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            var valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(38, 47, 58),
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 2)
            };
            var labelText = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(109, 120, 134),
                TextAlign = ContentAlignment.TopLeft
            };

            textLayout.Controls.Add(valueLabel, 0, 0);
            textLayout.Controls.Add(labelText, 0, 1);
            layout.Controls.Add(mark, 0, 0);
            layout.Controls.Add(accent, 1, 0);
            layout.Controls.Add(textLayout, 2, 0);

            panel.Controls.Add(layout);
            return (panel, valueLabel, labelText);
        }

        private static Panel CreateJournalHealthCard(Action navigateToJournal)
        {
            var transactions = JournalTransactionDataAccess.GetAll();
            var totalDebits = transactions.Sum(transaction => transaction.Debit);
            var totalCredits = transactions.Sum(transaction => transaction.Credit);
            var difference = Math.Abs(totalDebits - totalCredits);
            var isBalanced = difference == 0m;
            var currencyFormat = CultureInfo.GetCultureInfo("en-IN");

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Width = 520,
                Height = 226,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 0)
            };
            panel.Paint += (_, e) =>
            {
                using var borderPen = new Pen(Color.FromArgb(215, 222, 230));
                e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
                e.Graphics.DrawLine(borderPen, 20, 52, panel.Width - 20, 52);
            };

            var title = new Label
            {
                Text = "Journal Health",
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(42, 50, 59),
                Location = new Point(22, 18)
            };

            var viewLink = new Label
            {
                Text = "Review journal",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(54, 107, 125),
                Location = new Point(390, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            viewLink.Click += (_, _) => navigateToJournal();

            var table = new TableLayoutPanel
            {
                Location = new Point(22, 62),
                Size = new Size(476, 88),
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            for (var column = 0; column < 4; column++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var metrics = new[]
            {
                new { Label = "Entries", Value = transactions.Count.ToString("N0", currencyFormat) },
                new { Label = "Debits", Value = $"₹{totalDebits.ToString("#,##0.00", currencyFormat)}" },
                new { Label = "Credits", Value = $"₹{totalCredits.ToString("#,##0.00", currencyFormat)}" },
                new { Label = "Difference", Value = $"₹{difference.ToString("#,##0.00", currencyFormat)}" }
            };

            for (var i = 0; i < metrics.Length; i++)
            {
                var metricLabel = new Label
                {
                    Text = metrics[i].Label,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(109, 120, 134),
                    TextAlign = ContentAlignment.BottomLeft
                };
                var metricValue = new Label
                {
                    Text = metrics[i].Value,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = i == 3 && !isBalanced ? Color.FromArgb(176, 73, 64) : Color.FromArgb(42, 50, 59),
                    TextAlign = ContentAlignment.TopLeft
                };

                table.Controls.Add(metricLabel, i, 0);
                table.Controls.Add(metricValue, i, 1);
            }

            var status = new Label
            {
                Text = isBalanced ? "Journal is balanced" : "Out of balance - review required",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = isBalanced ? Color.FromArgb(54, 125, 91) : Color.FromArgb(176, 73, 64),
                Location = new Point(22, 165)
            };

            panel.Controls.Add(title);
            panel.Controls.Add(viewLink);
            panel.Controls.Add(table);
            panel.Controls.Add(status);
            return panel;
        }

        private static (decimal TotalPayables, decimal TotalOverdue) GetPayableTotals()
        {
            var payableTransactions = JournalTransactionDataAccess.GetAll()
                .Where(transaction =>
                    transaction.Particulars.Contains("supplier", StringComparison.OrdinalIgnoreCase)
                    || transaction.Particulars.Contains("vendor", StringComparison.OrdinalIgnoreCase)
                    || transaction.Particulars.Contains("payable", StringComparison.OrdinalIgnoreCase)
                    || transaction.Particulars.Contains("creditor", StringComparison.OrdinalIgnoreCase))
                .GroupBy(transaction => transaction.Particulars.Trim(), StringComparer.OrdinalIgnoreCase);

            var totalPayables = 0m;
            var totalOverdue = 0m;
            var today = DateTime.Today;

            foreach (var account in payableTransactions)
            {
                var outstandingCredits = new List<(decimal Amount, DateTime? EntryDate)>();
                var prepayment = 0m;

                foreach (var transaction in account
                    .OrderBy(x => x.EntryDate ?? DateTime.MaxValue)
                    .ThenBy(x => x.Id))
                {
                    var debit = transaction.Debit;
                    for (var index = 0; index < outstandingCredits.Count && debit > 0m; index++)
                    {
                        var offset = Math.Min(outstandingCredits[index].Amount, debit);
                        outstandingCredits[index] = (outstandingCredits[index].Amount - offset, outstandingCredits[index].EntryDate);
                        debit -= offset;
                    }

                    outstandingCredits.RemoveAll(credit => credit.Amount == 0m);
                    prepayment += debit;

                    var creditAmount = transaction.Credit;
                    var prepaymentOffset = Math.Min(creditAmount, prepayment);
                    creditAmount -= prepaymentOffset;
                    prepayment -= prepaymentOffset;
                    if (creditAmount > 0m)
                    {
                        outstandingCredits.Add((creditAmount, transaction.EntryDate));
                    }
                }

                foreach (var outstandingCredit in outstandingCredits)
                {
                    totalPayables += outstandingCredit.Amount;
                    if (outstandingCredit.EntryDate.HasValue && outstandingCredit.EntryDate.Value.Date < today)
                    {
                        totalOverdue += outstandingCredit.Amount;
                    }
                }
            }

            return (totalPayables, totalOverdue);
        }
    }
}
