using System;
using System.Windows.Forms;
using RepairAndMaintenanceApp.DataAccess;

namespace RepairAndMaintenanceApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SqliteDataAccess.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DashboardForm());
        }
    }
}