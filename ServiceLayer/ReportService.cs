using System;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public static class ReportService
    {
        public const string MonthlySummary = "Monthly Income vs Expense";
        public const string CategorySummary = "Category-wise Summary";
        public const string DayBook = "Day Book";
        public const string TrialBalance = "Trial Balance";

        public static readonly string[] ReportNames = { MonthlySummary, CategorySummary, DayBook, TrialBalance };

        public static ReportTable Generate(string reportName, DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                throw new ArgumentException("The From date must not be after the To date.");
            }

            return reportName switch
            {
                MonthlySummary => ReportDataAccess.GetMonthlyIncomeExpenseSummary(fromDate, toDate),
                CategorySummary => ReportDataAccess.GetCategorySummary(fromDate, toDate),
                DayBook => ReportDataAccess.GetDayBook(fromDate, toDate),
                TrialBalance => ReportDataAccess.GetTrialBalance(fromDate, toDate),
                _ => throw new ArgumentException($"Unknown report: {reportName}")
            };
        }
    }
}
