using System;
using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class JournalTransactionService
    {
        public static List<JournalTransaction> GetAll(DateTime? selectedMonth = null, string? particulars = null)
        {
            return JournalTransactionDataAccess.GetAll(selectedMonth, particulars);
        }

        public static List<DateTime> GetDistinctMonths()
        {
            return JournalTransactionDataAccess.GetDistinctMonths();
        }

        public static List<BalanceSheetItems> GetYearlyBalanceSheet(int year)
        {
            return JournalTransactionDataAccess.GetYearlyBalanceSheet(year);
        }

        public static List<int> GetDistinctYears()
        {
            return JournalTransactionDataAccess.GetDistinctYears();
        }

        public static List<BalanceSheetItems> GetMonthlyBalanceSheet(DateTime selectedMonth)
        {
            return JournalTransactionDataAccess.GetMonthlyBalanceSheet(selectedMonth);
        }

        public static List<JournalParticularBalance> GetMonthlyBalances(DateTime selectedMonth, string? particulars = null)
        {
            return JournalTransactionDataAccess.GetMonthlyBalances(selectedMonth, particulars);
        }

        public static List<string> GetDistinctParticulars()
        {
            return JournalTransactionDataAccess.GetDistinctParticulars();
        }
    }
}
