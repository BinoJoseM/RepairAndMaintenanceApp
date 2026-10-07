using System;
using System.Drawing;
using System.Windows.Forms;
using RepairAndMaintenanceApp.ServiceLayer;

namespace RepairAndMaintenanceApp
{
    public class SettingsForm : Form
    {
        public SettingsForm()
        {
            Text = "Settings";
            BackColor = Color.FromArgb(244, 246, 249);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 46, 58),
                AutoSize = true,
                Location = new Point(24, 24)
            };

            var backupPanel = new Panel
            {
                Location = new Point(24, 82),
                Size = new Size(700, 150),
                BackColor = Color.White,
                Padding = new Padding(20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var backupTitle = new Label
            {
                Text = "Database Backup",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 46, 58),
                AutoSize = true,
                Location = new Point(20, 18)
            };

            var description = new Label
            {
                Text = "Choose a folder to save a timestamped copy of the current database.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(109, 120, 134),
                AutoSize = true,
                Location = new Point(20, 52)
            };

            var backupButton = new Button
            {
                Text = "Back Up Database",
                Size = new Size(170, 38),
                Location = new Point(20, 88),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(79, 100, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            backupButton.FlatAppearance.BorderSize = 0;
            backupButton.Click += (_, _) => CreateBackup();

            backupPanel.Controls.Add(backupTitle);
            backupPanel.Controls.Add(description);
            backupPanel.Controls.Add(backupButton);
            Controls.Add(title);
            Controls.Add(backupPanel);

            Resize += (_, _) => backupPanel.Width = Math.Max(300, ClientSize.Width - 48);
        }

        private void CreateBackup()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select a folder for the database backup. You can choose a OneDrive folder.",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                var backupPath = DatabaseBackupService.CreateBackup(dialog.SelectedPath);
                AppMessageBox.Show($"Database backup created successfully:\n{backupPath}", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                AppMessageBox.Show($"The database backup could not be created.\n\n{exception.Message}", "Backup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
