using System.ComponentModel;
using System.Windows.Forms;

namespace RepairAndMaintenanceApp
{
    partial class MainForm
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.Text = "Main Form";
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Additional UI components can be initialized here
        }
    }
}