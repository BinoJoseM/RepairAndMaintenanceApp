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
