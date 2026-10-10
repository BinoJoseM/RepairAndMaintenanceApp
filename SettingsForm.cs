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

            var settings = BackupSettingsService.Load();

            var backupPanel = new Panel
            {
                Location = new Point(24, 82),
                Size = new Size(700, 270),
                BackColor = Color.White,
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
                Text = "Choose a backup folder (a OneDrive folder works well). Each backup is a timestamped copy.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(109, 120, 134),
                AutoSize = true,
                Location = new Point(20, 52)
            };

            var folderBox = new TextBox
            {
                Location = new Point(20, 88),
                Width = 450,
                ReadOnly = true,
                BackColor = Color.White,
                Text = settings.BackupFolder
            };

            var browseButton = CreateButton("Browse...", new Point(486, 85), 110, Color.FromArgb(230, 236, 241), Color.FromArgb(60, 72, 84));
            var backupButton = CreateButton("Back Up Now", new Point(20, 130), 170, Color.FromArgb(79, 100, 135), Color.White);

            var autoBackupBox = new CheckBox
            {
                Text = "Automatically back up when the app closes",
                AutoSize = true,
                Location = new Point(20, 184),
                Checked = settings.AutoBackupOnExit,
                ForeColor = Color.FromArgb(60, 72, 84)
            };

            lastBackupLabel = new Label
            {
                AutoSize = true,
                Location = new Point(20, 220),
                ForeColor = Color.FromArgb(109, 120, 134)
            };
            UpdateLastBackupLabel();

            browseButton.Click += (_, _) =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select a folder for database backups. You can choose a OneDrive folder.",
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true,
                    SelectedPath = folderBox.Text
                };

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                folderBox.Text = dialog.SelectedPath;
                var current = BackupSettingsService.Load();
                current.BackupFolder = dialog.SelectedPath;
                BackupSettingsService.Save(current);
            };

            backupButton.Click += (_, _) => CreateBackup(folderBox);

            autoBackupBox.CheckedChanged += (_, _) =>
            {
                if (autoBackupBox.Checked && string.IsNullOrWhiteSpace(folderBox.Text))
                {
                    AppMessageBox.Show("Select a backup folder first.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    autoBackupBox.Checked = false;
                    return;
                }

                var current = BackupSettingsService.Load();
                current.AutoBackupOnExit = autoBackupBox.Checked;
                BackupSettingsService.Save(current);
            };

            backupPanel.Controls.Add(backupTitle);
            backupPanel.Controls.Add(description);
            backupPanel.Controls.Add(folderBox);
            backupPanel.Controls.Add(browseButton);
            backupPanel.Controls.Add(backupButton);
            backupPanel.Controls.Add(autoBackupBox);
            backupPanel.Controls.Add(lastBackupLabel);
            Controls.Add(title);
            Controls.Add(backupPanel);

            Resize += (_, _) => backupPanel.Width = Math.Max(300, ClientSize.Width - 48);
        }

        private Label lastBackupLabel = null!;

        private void UpdateLastBackupLabel()
        {
            var last = BackupSettingsService.Load().LastBackupTime;
            lastBackupLabel.Text = last.HasValue ? $"Last backup: {last.Value:dd MMM yyyy HH:mm}" : "Last backup: never";
        }

        private static Button CreateButton(string text, Point location, int width, Color back, Color fore)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(width, 38),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void CreateBackup(TextBox folderBox)
        {
            if (string.IsNullOrWhiteSpace(folderBox.Text))
            {
                AppMessageBox.Show("Select a backup folder first (Browse...).", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var backupPath = BackupSettingsService.CreateBackupAndRecord(folderBox.Text);
                UpdateLastBackupLabel();
                AppMessageBox.Show($"Database backup created successfully:\n{backupPath}", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                AppMessageBox.Show($"The database backup could not be created.\n\n{exception.Message}", "Backup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}