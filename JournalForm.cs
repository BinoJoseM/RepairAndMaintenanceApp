using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

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

            var monthAndDateLabel = new Label
            {
                Text = "MonthAndDate",
                AutoSize = true,
                Location = new Point(14, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var monthAndDateBox = new DateTimePicker
            {
                Width = 150,
                Location = new Point(112, 15),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                ShowCheckBox = true,
                Checked = false
            };

            var accountLabel = new Label
            {
                Text = "Account",
                AutoSize = true,
                Location = new Point(282, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var accountSearchBox = new ComboBox
            {
                Width = 155,
                Location = new Point(354, 14),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            accountSearchBox.Items.AddRange(new object[]
            {
                "All accounts",
                "Cash",
                "Office Supplies",
                "Accounts Receivable",
                "Maintenance Expense",
                "Bank"
            });
            accountSearchBox.SelectedIndex = 0;

            var searchButton = CreateActionButton("Search", 530, 13, 90, Color.FromArgb(32, 74, 140), Color.White);
            var clearButton = CreateActionButton("Clear", 630, 13, 80, Color.FromArgb(232, 236, 240), Color.FromArgb(60, 72, 84));

            var grid = new DataGridView
            {
                Location = new Point(24, 134),
                Size = new Size(900, 438),
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

            grid.Columns.Add("Date", "Date");
            grid.Columns.Add("Ref", "Reference");
            grid.Columns.Add("Account", "Account");
            grid.Columns.Add("Debit", "Debit");
            grid.Columns.Add("Credit", "Credit");
            grid.Columns.Add("Notes", "Notes");

            grid.Rows.Add("2026-08-03", "JV-101", "Cash", "$6,200.00", "", "Opening balance");
            grid.Rows.Add("2026-08-05", "JV-102", "Office Supplies", "$420.00", "", "Stationery");
            grid.Rows.Add("2026-08-09", "JV-103", "Accounts Receivable", "", "$1,450.00", "Service invoice");
            grid.Rows.Add("2026-08-12", "JV-104", "Maintenance Expense", "$1,930.00", "", "Equipment service");
            grid.Rows.Add("2026-08-18", "JV-105", "Bank", "", "$2,100.00", "Deposit received");

            var editorPanel = new Panel
            {
                Location = new Point(24, 584),
                Size = new Size(900, 165),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var editorTitle = new Label
            {
                Text = "Save / Update Journal Entry",
                AutoSize = true,
                Location = new Point(14, 10),
                ForeColor = Color.FromArgb(32, 74, 140),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var dateBox = new DateTimePicker
            {
                Location = new Point(14, 56),
                Width = 125,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd"
            };
            var referenceBox = CreateEditorTextBox(235, 56, 110);
            var accountBox = CreateEditorTextBox(365, 56, 165);
            var debitBox = CreateEditorTextBox(550, 56, 110);
            var creditBox = CreateEditorTextBox(675, 56, 110);
            var notesBox = CreateEditorTextBox(14, 116, 300);

            AddEditorLabel(editorPanel, "Date", 14, 35);
            AddEditorLabel(editorPanel, "Reference", 235, 35);
            AddEditorLabel(editorPanel, "Account", 365, 35);
            AddEditorLabel(editorPanel, "Debit", 550, 35);
            AddEditorLabel(editorPanel, "Credit", 675, 35);
            AddEditorLabel(editorPanel, "Notes", 14, 95);

            var addNewButton = CreateActionButton("Add New", 330, 108, 95, Color.FromArgb(70, 120, 75), Color.White);
            var saveButton = CreateActionButton("Save", 435, 108, 85, Color.FromArgb(32, 74, 140), Color.White);
            var updateButton = CreateActionButton("Update", 530, 108, 90, Color.FromArgb(0, 114, 180), Color.White);
            var deleteButton = CreateActionButton("Delete Row", 630, 108, 105, Color.FromArgb(180, 55, 55), Color.White);

            void LoadSelectedRow()
            {
                if (grid.CurrentRow == null)
                {
                    return;
                }

                if (DateTime.TryParseExact(grid.CurrentRow.Cells[0].Value?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var entryDate))
                {
                    dateBox.Value = entryDate;
                }

                referenceBox.Text = grid.CurrentRow.Cells[1].Value?.ToString();
                accountBox.Text = grid.CurrentRow.Cells[2].Value?.ToString();
                debitBox.Text = grid.CurrentRow.Cells[3].Value?.ToString();
                creditBox.Text = grid.CurrentRow.Cells[4].Value?.ToString();
                notesBox.Text = grid.CurrentRow.Cells[5].Value?.ToString();
            }

            bool TryValidateEditor()
            {
                if (string.IsNullOrWhiteSpace(referenceBox.Text) || string.IsNullOrWhiteSpace(accountBox.Text) || string.IsNullOrWhiteSpace(notesBox.Text))
                {
                    MessageBox.Show("Enter the reference, account, and notes.", "Journal Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var debit = ParseAmount(debitBox.Text);
                var credit = ParseAmount(creditBox.Text);
                if (debit == null || credit == null || (debit == 0 && credit == 0) || (debit > 0 && credit > 0))
                {
                    MessageBox.Show("Enter either a debit or a credit amount.", "Journal Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                debitBox.Text = debit == 0 ? string.Empty : debit.Value.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                creditBox.Text = credit == 0 ? string.Empty : credit.Value.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                return true;
            }

            addNewButton.Click += (_, _) =>
            {
                grid.ClearSelection();
                dateBox.Value = DateTime.Today;
                referenceBox.Clear();
                accountBox.Clear();
                debitBox.Clear();
                creditBox.Clear();
                notesBox.Clear();
                referenceBox.Focus();
            };

            saveButton.Click += (_, _) =>
            {
                if (!TryValidateEditor())
                {
                    return;
                }

                grid.Rows.Add(dateBox.Value.ToString("yyyy-MM-dd"), referenceBox.Text.Trim(), accountBox.Text.Trim(), debitBox.Text, creditBox.Text, notesBox.Text.Trim());
                MessageBox.Show("Journal entry saved.", "Journal Entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            updateButton.Click += (_, _) =>
            {
                if (!TryValidateEditor() || grid.CurrentRow == null)
                {
                    return;
                }

                grid.CurrentRow.SetValues(dateBox.Value.ToString("yyyy-MM-dd"), referenceBox.Text.Trim(), accountBox.Text.Trim(), debitBox.Text, creditBox.Text, notesBox.Text.Trim());
                MessageBox.Show("Journal entry updated.", "Journal Entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            deleteButton.Click += (_, _) =>
            {
                if (grid.CurrentRow == null)
                {
                    return;
                }

                var confirmation = MessageBox.Show("Delete the selected journal entry?", "Delete Journal Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirmation == DialogResult.Yes)
                {
                    grid.Rows.Remove(grid.CurrentRow);
                    MessageBox.Show("Journal entry deleted.", "Journal Entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            void ApplySearch()
            {
                var selectedDate = monthAndDateBox.Value.ToString("yyyy-MM-dd");
                var selectedAccount = accountSearchBox.SelectedItem?.ToString() ?? "All accounts";

                foreach (DataGridViewRow row in grid.Rows)
                {
                    var matchesDate = !monthAndDateBox.Checked || row.Cells[0].Value?.ToString() == selectedDate;
                    var matchesAccount = selectedAccount == "All accounts" || row.Cells[2].Value?.ToString() == selectedAccount;
                    row.Visible = matchesDate && matchesAccount;
                }
            }

            searchButton.Click += (_, _) => ApplySearch();
            clearButton.Click += (_, _) =>
            {
                monthAndDateBox.Checked = false;
                accountSearchBox.SelectedIndex = 0;
                ApplySearch();
            };

            grid.SelectionChanged += (_, _) => LoadSelectedRow();

            searchPanel.Controls.Add(monthAndDateLabel);
            searchPanel.Controls.Add(monthAndDateBox);
            searchPanel.Controls.Add(accountLabel);
            searchPanel.Controls.Add(accountSearchBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(clearButton);

            editorPanel.Controls.Add(editorTitle);
            editorPanel.Controls.Add(dateBox);
            editorPanel.Controls.Add(referenceBox);
            editorPanel.Controls.Add(accountBox);
            editorPanel.Controls.Add(debitBox);
            editorPanel.Controls.Add(creditBox);
            editorPanel.Controls.Add(notesBox);
            editorPanel.Controls.Add(addNewButton);
            editorPanel.Controls.Add(saveButton);
            editorPanel.Controls.Add(updateButton);
            editorPanel.Controls.Add(deleteButton);

            Controls.Add(title);
            Controls.Add(searchPanel);
            Controls.Add(grid);
            Controls.Add(editorPanel);

            LoadSelectedRow();
        }

        private static decimal? ParseAmount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            return decimal.TryParse(text.Replace("$", string.Empty).Replace(",", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
                ? amount
                : null;
        }

        private static TextBox CreateEditorTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width
            };
        }

        private static void AddEditorLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, y),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            });
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
                ForeColor = foreColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
