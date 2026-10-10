using System;
using System.Drawing;
using System.Windows.Forms;
using RepairAndMaintenanceApp.Entities;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class CategoryMasterForm : Form
    {
        private readonly DataGridView grid;
        private readonly TextBox searchBox;
        private readonly TextBox categoryNameBox;
        private readonly ComboBox categoryTypeBox;
        private readonly CheckBox activeBox;
        private int? selectedCategoryId;

        public CategoryMasterForm()
        {
            Text = "Category Master";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 246, 249);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Category Master",
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

            var searchLabel = new Label
            {
                Text = "Search Category",
                AutoSize = true,
                Location = new Point(14, 19),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 72, 84)
            };

            searchBox = new TextBox
            {
                Location = new Point(125, 15),
                Width = 220
            };

            var searchButton = CreateButton("Search", 365, 13, 90, Color.FromArgb(32, 74, 140), Color.White);
            var clearButton = CreateButton("Clear", 465, 13, 80, Color.FromArgb(232, 236, 240), Color.FromArgb(60, 72, 84));

            searchPanel.Controls.Add(searchLabel);
            searchPanel.Controls.Add(searchBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(clearButton);

            grid = new DataGridView
            {
                Location = new Point(24, 134),
                Size = new Size(900, 300),
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
                AutoGenerateColumns = false,
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
            grid.Columns.Add("Id", "Id");
            grid.Columns.Add("CategoryName", "Category");
            grid.Columns.Add("CategoryType", "Type");
            grid.Columns.Add("Active", "Active");

            var editorPanel = new Panel
            {
                Location = new Point(24, 450),
                Size = new Size(900, 125),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var editorTitle = new Label
            {
                Text = "Add / Update Category",
                AutoSize = true,
                Location = new Point(14, 10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 74, 140)
            };

            categoryNameBox = new TextBox
            {
                Location = new Point(14, 57),
                Width = 220
            };

            categoryTypeBox = new ComboBox
            {
                Location = new Point(255, 56),
                Width = 145,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            categoryTypeBox.Items.AddRange(new object[] { "Income", "Expense" });
            categoryTypeBox.SelectedIndex = 0;

            activeBox = new CheckBox
            {
                Text = "Active",
                Location = new Point(420, 57),
                AutoSize = true,
                Checked = true
            };

            var addNewButton = CreateButton("Add New", 515, 52, 90, Color.FromArgb(70, 120, 75), Color.White);
            var saveButton = CreateButton("Save", 615, 52, 75, Color.FromArgb(32, 74, 140), Color.White);
            var updateButton = CreateButton("Update", 700, 52, 75, Color.FromArgb(0, 114, 180), Color.White);
            var deleteButton = CreateButton("Delete", 785, 52, 75, Color.FromArgb(180, 55, 55), Color.White);

            AddLabel(editorPanel, "Category", 14, 36);
            AddLabel(editorPanel, "Type", 255, 36);
            editorPanel.Controls.Add(editorTitle);
            editorPanel.Controls.Add(categoryNameBox);
            editorPanel.Controls.Add(categoryTypeBox);
            editorPanel.Controls.Add(activeBox);
            editorPanel.Controls.Add(addNewButton);
            editorPanel.Controls.Add(saveButton);
            editorPanel.Controls.Add(updateButton);
            editorPanel.Controls.Add(deleteButton);

            searchButton.Click += (_, _) => LoadCategories(searchBox.Text.Trim());
            clearButton.Click += (_, _) =>
            {
                searchBox.Clear();
                LoadCategories();
            };
            addNewButton.Click += (_, _) => ClearEditor();
            saveButton.Click += (_, _) => SaveCategory();
            updateButton.Click += (_, _) => UpdateCategory();
            deleteButton.Click += (_, _) => DeleteCategory();
            grid.SelectionChanged += (_, _) => LoadSelectedCategory();

            Controls.Add(title);
            Controls.Add(searchPanel);
            Controls.Add(grid);
            Controls.Add(editorPanel);

            LoadCategories();
        }

        private void LoadCategories(string searchText = "")
        {
            grid.Rows.Clear();
            foreach (var category in CategoryMasterService.GetAll(searchText))
            {
                grid.Rows.Add(category.Id, category.CategoryName, category.CategoryType, category.IsActive ? "Yes" : "No");
            }

            grid.ClearSelection();
            selectedCategoryId = null;
        }

        private void LoadSelectedCategory()
        {
            if (grid.CurrentRow == null)
            {
                return;
            }

            selectedCategoryId = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            categoryNameBox.Text = grid.CurrentRow.Cells["CategoryName"].Value?.ToString() ?? string.Empty;
            categoryTypeBox.SelectedItem = grid.CurrentRow.Cells["CategoryType"].Value?.ToString();
            activeBox.Checked = string.Equals(grid.CurrentRow.Cells["Active"].Value?.ToString(), "Yes", StringComparison.OrdinalIgnoreCase);
        }

        private void ClearEditor()
        {
            selectedCategoryId = null;
            grid.ClearSelection();
            categoryNameBox.Clear();
            categoryTypeBox.SelectedIndex = 0;
            activeBox.Checked = true;
            categoryNameBox.Focus();
        }

        private void SaveCategory()
        {
            if (!ValidateEditor())
            {
                return;
            }

            try
            {
                CategoryMasterService.Add(new CategoryMaster
                {
                    CategoryName = categoryNameBox.Text.Trim(),
                    CategoryType = categoryTypeBox.Text,
                    IsActive = activeBox.Checked
                });
                LoadCategories(searchBox.Text.Trim());
                AppMessageBox.Show("Category saved.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                AppMessageBox.Show("That category already exists.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateCategory()
        {
            if (!selectedCategoryId.HasValue || !ValidateEditor())
            {
                return;
            }

            try
            {
                CategoryMasterService.Update(new CategoryMaster
                {
                    Id = selectedCategoryId.Value,
                    CategoryName = categoryNameBox.Text.Trim(),
                    CategoryType = categoryTypeBox.Text,
                    IsActive = activeBox.Checked
                });
                LoadCategories(searchBox.Text.Trim());
                AppMessageBox.Show("Category updated.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                AppMessageBox.Show("That category already exists.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteCategory()
        {
            if (!selectedCategoryId.HasValue)
            {
                return;
            }

            if (CategoryMasterService.IsUsedInParticulars(selectedCategoryId.Value))
            {
                AppMessageBox.Show("This category is mapped to particulars and cannot be deleted.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmation = AppMessageBox.Show("Delete the selected category?", "Delete Category", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                CategoryMasterService.Delete(selectedCategoryId.Value);
                LoadCategories(searchBox.Text.Trim());
                AppMessageBox.Show("Category deleted.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                AppMessageBox.Show("This category is used by ledger entries or particulars. Deactivate it instead of deleting it.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateEditor()
        {
            if (string.IsNullOrWhiteSpace(categoryNameBox.Text))
            {
                AppMessageBox.Show("Enter a category name.", "Category Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                categoryNameBox.Focus();
                return false;
            }

            return true;
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
