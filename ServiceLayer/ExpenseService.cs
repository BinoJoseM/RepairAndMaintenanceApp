using System;
using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class ExpenseService
    {
        public static List<ExpenseLedgerEntries> GetAll(string? category = null, DateTime? selectedDate = null)
        {
            return ExpenseLedgerDataAccess.GetAll(category, selectedDate);
        }

        public static void Add(ExpenseLedgerEntries entry)
        {
            SaveManualEntry(entry);
        }

        public static void SaveManualEntry(ExpenseLedgerEntries entry)
        {
            ExpenseLedgerDataAccess.SaveManualEntry(entry);
        }

        public static void Update(ExpenseLedgerEntries entry)
        {
            UpdateManualEntry(entry);
        }

        public static void UpdateManualEntry(ExpenseLedgerEntries entry)
        {
            ExpenseLedgerDataAccess.UpdateManualEntry(entry);
        }

        public static void Delete(int id)
        {
            ExpenseLedgerDataAccess.Delete(id);
        }
    }
}
