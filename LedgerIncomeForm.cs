using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class LedgerIncomeForm : Form
    {
        public LedgerIncomeForm()
        {
            Text = "Ledger (Income)";
            Width = 980;
            Height = 820;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            MinimumSize = new Size(980, 760);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Ledger (Income)",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20),
                ForeColor = Color.FromArgb(40, 46, 58)
            };

            var searchPanel = new Panel
            {
                Location = new Point(24, 62),
                Size = new Size(900, 58),
                BackColor = Color.FromArgb(255, 255, 255),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12, 10, 12, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var monthAndDateLabel = new Label
            {
                Text = "Month",
                AutoSize = true,
                Location = new Point(8, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var monthAndDateBox = new DateTimePicker
            {
                Width = 150,
                Location = new Point(108, 15),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMMM yyyy",
                ShowCheckBox = true,
                Checked = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            var categoriesLabel = new Label
            {
                Text = "Categories",
                AutoSize = true,
                Location = new Point(278, 19),
                ForeColor = Color.FromArgb(60, 72, 84),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var categoriesBox = new ComboBox
            {
                Width = 155,
                Location = new Point(350, 14),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(40, 46, 58)
            };

            var categoryItems = new List<string> { "All categories" };
            categoryItems.AddRange(SqliteDataAccess.GetCategoryNames("Income"));
            categoriesBox.Items.AddRange(categoryItems.Distinct().ToArray());
            categoriesBox.SelectedIndex = 0;

            var searchButton = CreateActionButton("Search", 530, 13, 90, Color.FromArgb(79, 100, 135), Color.White);
            var clearButton = CreateActionButton("Clear", 630, 13, 80, Color.FromArgb(230, 236, 241), Color.FromArgb(60, 72, 84));

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
                Font = new Font("Segoe UI", 10F),
                GridColor = Color.FromArgb(223, 230, 239),
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                AutoGenerateColumns = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 34 },
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
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

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(2, 0, 2, 0)
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(247, 249, 251),
                ForeColor = Color.FromArgb(42, 50, 59)
            };

            grid.Columns.Add("Date", "Date");
            grid.Columns.Add("Category", "Category");
            grid.Columns.Add("Particulars", "Particulars");
            grid.Columns.Add("Amount", "Amount");
            // grid.Columns.Add("SourceFile", "Source File");
            // grid.Columns.Add("SourceCell", "Source Cell");

            void LoadGridData()
            {
                grid.Rows.Clear();

                var entries = IncomeLedgerService.GetAll();
                foreach (var item in entries)
                {
                    var rowIndex = grid.Rows.Add(
                        item.EntryDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                        item.Category,
                        item.Particulars,
                        item.Amount.ToString("C2", CultureInfo.GetCultureInfo("en-US")));
                        // item.SourceFile,
                        // item.SourceCell

                    grid.Rows[rowIndex].Tag = item.Id;
                }

                grid.ClearSelection();
                grid.CurrentCell = null;
            }

            LoadGridData();

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
                Text = "Save / Update Income Data",
                AutoSize = true,
                Location = new Point(14, 10),
                ForeColor = Color.FromArgb(54, 107, 125),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var dateBox = new DateTimePicker
            {
                Location = new Point(14, 56),
                Width = 125,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };
            var particularsBox = CreateEditorTextBox(235, 56, 205);
            var amountBox = CreateEditorTextBox(536, 56, 115);

            var categoryBox = CreateDropDown(675, 55, 190, SqliteDataAccess.GetCategoryNames("Income").ToArray());

            AddEditorLabel(editorPanel, "Date", 14, 35);
            AddEditorLabel(editorPanel, "Particulars", 235, 35);
            AddEditorLabel(editorPanel, "Amount", 536, 35);
            AddEditorLabel(editorPanel, "Category", 675, 35);

            var addNewButton = CreateActionButton("Add New", 420, 108, 95, Color.FromArgb(70, 120, 75), Color.White);
            var saveButton = CreateActionButton("Save", 526, 108, 95, Color.FromArgb(79, 100, 135), Color.White);
            var updateButton = CreateActionButton("Update", 632, 108, 95, Color.FromArgb(54, 107, 125), Color.White);
            var deleteButton = CreateActionButton("Delete Row", 738, 108, 110, Color.FromArgb(170, 80, 72), Color.White);

            void LoadSelectedRow()
            {
                if (grid.CurrentRow == null)
                {
                    return;
                }

                var selectedRowId = grid.CurrentRow.Tag as int?;
                if (!selectedRowId.HasValue)
                {
                    return;
                }

                var selectedEntry = IncomeLedgerService.GetAll().FirstOrDefault(x => x.Id == selectedRowId.Value);
                if (selectedEntry == null)
                {
                    return;
                }

                if (selectedEntry.EntryDate.HasValue)
                {
                    dateBox.Value = selectedEntry.EntryDate.Value;
                }

                categoryBox.SelectedItem = selectedEntry.Category;
                particularsBox.Text = selectedEntry.Particulars;
                amountBox.Text = selectedEntry.Amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            }

            bool TryValidateEditor()
            {
                if (string.IsNullOrWhiteSpace(particularsBox.Text) || string.IsNullOrWhiteSpace(amountBox.Text))
                {
                    MessageBox.Show("Enter the date, particulars, and amount.", "Income Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!decimal.TryParse(amountBox.Text.Replace("$", string.Empty).Replace(",", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                {
                    MessageBox.Show("Enter a valid amount.", "Income Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                amountBox.Text = amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                return true;
            }

            addNewButton.Click += (_, _) =>
            {
                grid.ClearSelection();
                dateBox.Value = DateTime.Today;
                categoryBox.SelectedIndex = 0;
                particularsBox.Clear();
                amountBox.Clear();
                particularsBox.Focus();
            };

            saveButton.Click += (_, _) =>
            {
                if (!TryValidateEditor())
                {
                    return;
                }

                var amount = decimal.Parse(amountBox.Text.Replace("$", string.Empty).Replace(",", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture);
                var entry = new IncomeLedgerEntries
                {
                    EntryDate = dateBox.Value,
                    Category = categoryBox.Text,
                    Particulars = particularsBox.Text.Trim(),
                    Amount = amount,
                    SourceFile = "Manual Entry",
                    SourceCell = "Manual"
                };

                IncomeLedgerService.Add(entry);
                LoadGridData();
                MessageBox.Show("Income record saved.", "Income Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            updateButton.Click += (_, _) =>
            {
                if (!TryValidateEditor() || grid.CurrentRow == null)
                {
                    return;
                }

                var selectedRowId = grid.CurrentRow.Tag as int?;
                if (!selectedRowId.HasValue)
                {
                    return;
                }

                var amount = decimal.Parse(amountBox.Text.Replace("$", string.Empty).Replace(",", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture);
                var selectedEntry = IncomeLedgerService.GetAll().FirstOrDefault(x => x.Id == selectedRowId.Value);
                if (selectedEntry == null)
                {
                    return;
                }

                var updatedEntry = new IncomeLedgerEntries
                {
                    Id = selectedEntry.Id,
                    EntryDate = dateBox.Value,
                    Category = categoryBox.Text,
                    Particulars = particularsBox.Text.Trim(),
                    Amount = amount,
                    SourceFile = selectedEntry.SourceFile,
                    SourceCell = selectedEntry.SourceCell
                };

                IncomeLedgerService.Update(updatedEntry);
                LoadGridData();
                MessageBox.Show("Income record updated.", "Income Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            deleteButton.Click += (_, _) =>
            {
                if (grid.CurrentRow == null)
                {
                    return;
                }

                var selectedRowId = grid.CurrentRow.Tag as int?;
                if (!selectedRowId.HasValue)
                {
                    return;
                }

                var confirmation = MessageBox.Show("Delete the selected income record?", "Delete Income Record", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirmation == DialogResult.Yes)
                {
                    IncomeLedgerService.Delete(selectedRowId.Value);
                    LoadGridData();
                    MessageBox.Show("Income record deleted.", "Income Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            void ApplySearch()
            {
                var selectedCategory = categoriesBox.SelectedItem?.ToString() ?? "All categories";

                var entries = IncomeLedgerService.GetAll(selectedCategory, monthAndDateBox.Checked ? monthAndDateBox.Value : null);
                grid.Rows.Clear();

                foreach (var item in entries)
                {
                    var rowIndex = grid.Rows.Add(
                        item.EntryDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                        item.Category,
                        item.Particulars,
                        item.Amount.ToString("C2", CultureInfo.GetCultureInfo("en-US")),
                        item.SourceFile,
                        item.SourceCell);

                    grid.Rows[rowIndex].Tag = item.Id;
                }
            }

            searchButton.Click += (_, _) => ApplySearch();
            clearButton.Click += (_, _) =>
            {
                monthAndDateBox.Checked = false;
                categoriesBox.SelectedIndex = 0;
                LoadGridData();
            };

            grid.SelectionChanged += (_, _) => LoadSelectedRow();

            searchPanel.Controls.Add(monthAndDateLabel);
            searchPanel.Controls.Add(monthAndDateBox);
            searchPanel.Controls.Add(categoriesLabel);
            searchPanel.Controls.Add(categoriesBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(clearButton);

            editorPanel.Controls.Add(editorTitle);
            editorPanel.Controls.Add(dateBox);
            editorPanel.Controls.Add(particularsBox);
            editorPanel.Controls.Add(amountBox);
            editorPanel.Controls.Add(categoryBox);
            editorPanel.Controls.Add(addNewButton);
            editorPanel.Controls.Add(saveButton);
            editorPanel.Controls.Add(updateButton);
            editorPanel.Controls.Add(deleteButton);

            Controls.Add(title);
            Controls.Add(searchPanel);
            Controls.Add(grid);
            Controls.Add(editorPanel);

            Resize += (_, _) =>
            {
                var width = Math.Max(0, ClientSize.Width - 48);
                searchPanel.Width = width;
                grid.Width = width;
                editorPanel.Width = width;
            };

            dateBox.Value = DateTime.Today;
            categoryBox.SelectedIndex = 0;
            particularsBox.Clear();
            amountBox.Clear();
        }

        private static TextBox CreateEditorTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(42, 50, 59),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };
        }

        private static ComboBox CreateDropDown(int x, int y, int width, params string[] values)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(42, 50, 59),
                Font = new Font("Segoe UI", 9F)
            };
            comboBox.Items.AddRange(values);
            comboBox.SelectedIndex = 0;
            return comboBox;
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
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
