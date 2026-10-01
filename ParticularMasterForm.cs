using System;
using System.Drawing;
using System.Windows.Forms;
using RepairAndMaintenanceApp.Entities;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class ParticularMasterForm : Form
    {
        private readonly DataGridView grid;
        private readonly TextBox searchBox;
        private readonly TextBox particularNameBox;
        private readonly ComboBox categoryBox;
        private int? selectedParticularId;

        public ParticularMasterForm()
        {
            Text = "Particular Master";
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
                Text = "Particular Master",
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
                Padding = new Padding(12, 10, 12, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var searchLabel = new Label
            {
                Text = "Search Particular",
                AutoSize = true,
                Location = new Point(14, 19),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 72, 84)
            };
            searchBox = new TextBox
            {
                Location = new Point(145, 15),
                Width = 220
            };

            var searchButton = CreateButton("Search", 385, 13, 90, Color.FromArgb(32, 74, 140), Color.White);
            var clearButton = CreateButton("Clear", 485, 13, 80, Color.FromArgb(232, 236, 240), Color.FromArgb(60, 72, 84));
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
            grid.Columns.Add("CategoryId", "Category Id");
            grid.Columns.Add("CategoryName", "Category");
            grid.Columns.Add("ParticularName", "Particular");
            grid.Columns.Add("CreatedAt", "Created At");

            var editorPanel = new Panel
            {
                Location = new Point(24, 450),
                Size = new Size(900, 125),
                BackColor = Color.White,
                Padding = new Padding(12),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            var editorTitle = new Label
            {
                Text = "Add / Update Particular",
                AutoSize = true,
                Location = new Point(14, 10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 74, 140)
            };

            categoryBox = new ComboBox
            {
                Location = new Point(14, 57),
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            particularNameBox = new TextBox
            {
                Location = new Point(230, 57),
                Width = 210
            };

            var addNewButton = CreateButton("Add New", 460, 52, 90, Color.FromArgb(70, 120, 75), Color.White);
            var saveButton = CreateButton("Save", 560, 52, 70, Color.FromArgb(32, 74, 140), Color.White);
            var updateButton = CreateButton("Update", 640, 52, 75, Color.FromArgb(0, 114, 180), Color.White);
            var deleteButton = CreateButton("Delete", 725, 52, 75, Color.FromArgb(180, 55, 55), Color.White);

            AddLabel(editorPanel, "Category", 14, 36);
            AddLabel(editorPanel, "Particular", 230, 36);
            editorPanel.Controls.Add(editorTitle);
            editorPanel.Controls.Add(categoryBox);
            editorPanel.Controls.Add(particularNameBox);
            editorPanel.Controls.Add(addNewButton);
            editorPanel.Controls.Add(saveButton);
            editorPanel.Controls.Add(updateButton);
            editorPanel.Controls.Add(deleteButton);

            searchButton.Click += (_, _) => LoadParticulars(searchBox.Text.Trim());
            clearButton.Click += (_, _) =>
            {
                searchBox.Clear();
                LoadParticulars();
            };
            addNewButton.Click += (_, _) => ClearEditor();
            saveButton.Click += (_, _) => SaveParticular();
            updateButton.Click += (_, _) => UpdateParticular();
            deleteButton.Click += (_, _) => DeleteParticular();
            grid.SelectionChanged += (_, _) => LoadSelectedParticular();

            Controls.Add(title);
            Controls.Add(searchPanel);
            Controls.Add(grid);
            Controls.Add(editorPanel);

            LoadCategories();
            LoadParticulars();
        }

        private void LoadCategories()
        {
            var categories = ParticularMasterService.GetActiveCategories();
            categoryBox.DataSource = categories;
            categoryBox.DisplayMember = "CategoryName";
            categoryBox.ValueMember = "Id";
            categoryBox.SelectedIndex = categories.Count > 0 ? 0 : -1;
        }

        private void LoadParticulars(string searchText = "")
        {
            grid.Rows.Clear();
            foreach (var particular in ParticularMasterService.GetAll(searchText))
            {
                grid.Rows.Add(particular.Id, particular.CategoryId, particular.CategoryName, particular.ParticularName, particular.CreatedAt);
            }

            grid.Columns["Id"].Visible = false;
            grid.Columns["CategoryId"].Visible = false;
            grid.ClearSelection();
            selectedParticularId = null;
        }

        private void LoadSelectedParticular()
        {
            if (grid.CurrentRow == null)
            {
                return;
            }

            selectedParticularId = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            categoryBox.SelectedValue = Convert.ToInt32(grid.CurrentRow.Cells["CategoryId"].Value);
            particularNameBox.Text = grid.CurrentRow.Cells["ParticularName"].Value?.ToString() ?? string.Empty;
        }

        private void ClearEditor()
        {
            selectedParticularId = null;
            grid.ClearSelection();
            particularNameBox.Clear();
            if (categoryBox.Items.Count > 0)
            {
                categoryBox.SelectedIndex = 0;
            }
            particularNameBox.Focus();
        }

        private void SaveParticular()
        {
            if (!ValidateEditor())
            {
                return;
            }

            try
            {
                ParticularMasterService.Add(new ParticularMaster
                {
                    CategoryId = Convert.ToInt32(categoryBox.SelectedValue),
                    ParticularName = particularNameBox.Text.Trim()
                });
                LoadParticulars(searchBox.Text.Trim());
                MessageBox.Show("Particular saved.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("That particular already exists for the selected category.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateParticular()
        {
            if (!selectedParticularId.HasValue || !ValidateEditor())
            {
                return;
            }

            try
            {
                ParticularMasterService.Update(new ParticularMaster
                {
                    Id = selectedParticularId.Value,
                    CategoryId = Convert.ToInt32(categoryBox.SelectedValue),
                    ParticularName = particularNameBox.Text.Trim()
                });
                LoadParticulars(searchBox.Text.Trim());
                MessageBox.Show("Particular updated.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("That particular already exists for the selected category.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteParticular()
        {
            if (!selectedParticularId.HasValue)
            {
                return;
            }

            var confirmation = MessageBox.Show("Delete the selected particular?", "Delete Particular", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ParticularMasterService.Delete(selectedParticularId.Value);
                LoadParticulars(searchBox.Text.Trim());
                MessageBox.Show("Particular deleted.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This particular is used by ledger entries and cannot be deleted.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateEditor()
        {
            if (categoryBox.SelectedValue == null)
            {
                MessageBox.Show("Create or activate a category first.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                categoryBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(particularNameBox.Text))
            {
                MessageBox.Show("Enter a particular name.", "Particular Master", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                particularNameBox.Focus();
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
