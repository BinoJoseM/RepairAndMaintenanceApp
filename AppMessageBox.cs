using System;
using System.Drawing;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    public static class AppMessageBox
    {
        private static readonly Color HeaderColor = Color.FromArgb(79, 100, 135);
        private static readonly Color PageColor = Color.FromArgb(244, 246, 249);
        private static readonly Color TextColor = Color.FromArgb(40, 46, 58);
        private static readonly Color PrimaryColor = Color.FromArgb(68, 85, 120);
        private static readonly Color PrimaryHoverColor = Color.FromArgb(58, 73, 105);
        private static readonly Color SecondaryColor = Color.FromArgb(232, 235, 240);

        public static DialogResult Show(string text)
        {
            return Show(text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static DialogResult Show(string text, string caption)
        {
            return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return Show(text, caption, buttons, MessageBoxIcon.None);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            var (glyph, accent) = GetIconStyle(icon);

            using var dialog = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                ShowInTaskbar = false,
                BackColor = PageColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ClientSize = new Size(440, 200),
                KeyPreview = true
            };

            var border = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = HeaderColor };
            var content = new Panel { Dock = DockStyle.Fill, BackColor = PageColor };

            var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = HeaderColor };
            var title = new Label
            {
                Text = string.IsNullOrWhiteSpace(caption) ? "Repair & Maintenance App" : caption,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(title);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = PageColor
            };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 20, 20, 8), BackColor = PageColor };
            var message = new Label
            {
                Text = text,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            if (glyph.Length > 0)
            {
                var iconLabel = new Label
                {
                    Text = glyph,
                    ForeColor = Color.White,
                    BackColor = accent,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    Size = new Size(44, 44),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Left
                };
                var spacer = new Panel { Dock = DockStyle.Left, Width = 16, BackColor = PageColor };
                body.Controls.Add(message);
                body.Controls.Add(spacer);
                body.Controls.Add(iconLabel);
            }
            else
            {
                body.Controls.Add(message);
            }

            var specs = GetButtons(buttons);
            Button? firstButton = null;
            // RightToLeft flow places the first added control at the right edge.
            for (var i = specs.Length - 1; i >= 0; i--)
            {
                var (label, result, isPrimary) = specs[i];
                var button = CreateButton(label, isPrimary);
                button.DialogResult = result;
                footer.Controls.Add(button);
                if (isPrimary && firstButton == null)
                {
                    firstButton = button;
                }
            }

            content.Controls.Add(body);
            content.Controls.Add(footer);
            content.Controls.Add(header);
            border.Controls.Add(content);
            dialog.Controls.Add(border);

            var preferred = TextRenderer.MeasureText(text, message.Font, new Size(340, 0), TextFormatFlags.WordBreak);
            dialog.ClientSize = new Size(440, Math.Max(190, 48 + 60 + 28 + Math.Max(preferred.Height, 44) + 12));

            dialog.AcceptButton = firstButton;
            var cancelResult = Array.Find(specs, spec => spec.Result is DialogResult.Cancel or DialogResult.No or DialogResult.OK);
            if (cancelResult.Label != null)
            {
                foreach (Control control in footer.Controls)
                {
                    if (control is Button button && button.DialogResult == cancelResult.Result)
                    {
                        dialog.CancelButton = button;
                        break;
                    }
                }
            }

            var owner = Form.ActiveForm;
            return owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        }

        private static Button CreateButton(string text, bool isPrimary)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(88, 38),
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = isPrimary ? PrimaryColor : SecondaryColor,
                ForeColor = isPrimary ? Color.White : PrimaryColor,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = isPrimary ? PrimaryHoverColor : Color.FromArgb(220, 224, 231);
            return button;
        }

        private static (string Label, DialogResult Result, bool IsPrimary)[] GetButtons(MessageBoxButtons buttons)
        {
            return buttons switch
            {
                MessageBoxButtons.OKCancel => new[] { ("OK", DialogResult.OK, true), ("Cancel", DialogResult.Cancel, false) },
                MessageBoxButtons.YesNo => new[] { ("Yes", DialogResult.Yes, true), ("No", DialogResult.No, false) },
                MessageBoxButtons.YesNoCancel => new[] { ("Yes", DialogResult.Yes, true), ("No", DialogResult.No, false), ("Cancel", DialogResult.Cancel, false) },
                MessageBoxButtons.RetryCancel => new[] { ("Retry", DialogResult.Retry, true), ("Cancel", DialogResult.Cancel, false) },
                MessageBoxButtons.AbortRetryIgnore => new[] { ("Abort", DialogResult.Abort, false), ("Retry", DialogResult.Retry, true), ("Ignore", DialogResult.Ignore, false) },
                _ => new[] { ("OK", DialogResult.OK, true) }
            };
        }

        private static (string Glyph, Color Accent) GetIconStyle(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Error => ("✕", Color.FromArgb(180, 55, 55)),
                MessageBoxIcon.Warning => ("!", Color.FromArgb(214, 140, 30)),
                MessageBoxIcon.Information => ("i", HeaderColor),
                MessageBoxIcon.Question => ("?", HeaderColor),
                _ => (string.Empty, HeaderColor)
            };
        }
    }
}
