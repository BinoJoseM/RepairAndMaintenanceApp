using System;
using System.Drawing;
using System.Windows.Forms;
using RepairAndMaintenanceApp.DataAccess;

namespace RepairAndMaintenanceApp
{
    public partial class LoginForm : Form
    {
        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly Button btnLogin;

        public LoginForm()
        {
            Text = "Accounting Sign In";
            Width = 520;
            Height = 360;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(244, 246, 249);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            var outerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 249),
                Padding = new Padding(0)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(79, 100, 135)
            };

            var headerTitle = new Label
            {
                Text = "Accounting Workspace",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 18)
            };

            var closeButton = new Button
            {
                Text = "×",
                Size = new Size(28, 28),
                Location = new Point(468, 18),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 100, 135),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (_, _) => Close();

            header.Controls.Add(headerTitle);
            header.Controls.Add(closeButton);

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 246, 249),
                Padding = new Padding(30, 20, 30, 20)
            };

            var subTitle = new Label
            {
                Text = "Welcome back",
                ForeColor = Color.FromArgb(40, 46, 58),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 12)
            };

            var hint = new Label
            {
                Text = "Sign in to continue to your workspace",
                ForeColor = Color.FromArgb(109, 120, 134),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(32, 52)
            };

            var lblUsername = new Label
            {
                Text = "Username",
                ForeColor = Color.FromArgb(40, 46, 58),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 86)
            };

            var lblPassword = new Label
            {
                Text = "Password",
                ForeColor = Color.FromArgb(40, 46, 58),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 154)
            };

            txtUsername = new TextBox
            {
                Location = new Point(30, 108),
                Width = 420,
                Height = 34,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(40, 46, 58),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular)
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 176),
                Width = 420,
                Height = 34,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(40, 46, 58),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                UseSystemPasswordChar = true
            };

            btnLogin = new Button
            {
                Text = "SIGN IN",
                Location = new Point(30, 230),
                Width = 120,
                Height = 40,
                BackColor = Color.FromArgb(68, 85, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 73, 105);

            AcceptButton = btnLogin;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(162, 230),
                Width = 95,
                Height = 40,
                BackColor = Color.FromArgb(232, 235, 240),
                ForeColor = Color.FromArgb(68, 85, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnLogin.Click += BtnLogin_Click;
            btnCancel.Click += (_, _) => Close();
            FormClosed += (_, _) => Application.Exit();

            content.Controls.Add(subTitle);
            content.Controls.Add(hint);
            content.Controls.Add(lblUsername);
            content.Controls.Add(lblPassword);
            content.Controls.Add(txtUsername);
            content.Controls.Add(txtPassword);
            content.Controls.Add(btnLogin);
            content.Controls.Add(btnCancel);

            outerPanel.Controls.Add(content);
            outerPanel.Controls.Add(header);
            Controls.Add(outerPanel);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter both username and password.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var isValid = SqliteDataAccess.ValidateUser(txtUsername.Text.Trim(), txtPassword.Text.Trim());
            if (!isValid)
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var mainForm = new DashboardForm();
            mainForm.Show();
            Hide();
        }
    }
}
