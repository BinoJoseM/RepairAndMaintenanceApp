using System;
using System.IO;
using System.Windows.Forms;
using RepairAndMaintenanceApp.DataAccess;

namespace RepairAndMaintenanceApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => ReportFatalError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportFatalError(e.ExceptionObject as Exception);

            try
            {
                SqliteDataAccess.Initialize();
                Application.Run(new LoginForm());
            }
            catch (Exception exception)
            {
                ReportFatalError(exception);
            }
        }

        private static void ReportFatalError(Exception? exception)
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RepairAndMaintenanceApp",
                "error.log");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
            }

            MessageBox.Show(
                $"The application hit an error and could not continue.\n\n{exception?.Message}\n\nDetails were saved to:\n{logPath}",
                "Repair & Maintenance App",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
