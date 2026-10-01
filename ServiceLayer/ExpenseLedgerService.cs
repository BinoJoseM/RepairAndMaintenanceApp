using System;
using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class ExpenseLedgerService
    {
        public static List<ExpenseLedgerEntries> GetAll(string? category = null, DateTime? selectedDate = null)
        {
            return ExpenseLedgerDataAccess.GetAll(category, selectedDate);
        }

        public static void Add(ExpenseLedgerEntries entry)
        {
            ExpenseLedgerDataAccess.Add(entry);
        }

        public static void Update(ExpenseLedgerEntries entry)
        {
            ExpenseLedgerDataAccess.Update(entry);
        }

        public static void Delete(int id)
        {
            ExpenseLedgerDataAccess.Delete(id);
        }
    }
}
