using System;
using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class IncomeService
    {
        public static List<IncomeLedgerEntries> GetAll(string? category = null, DateTime? selectedDate = null)
        {
            return IncomeLedgerDataAccess.GetAll(category, selectedDate);
        }

        public static void Add(IncomeLedgerEntries entry)
        {
            IncomeLedgerDataAccess.Add(entry);
        }

        public static void Update(IncomeLedgerEntries entry)
        {
            IncomeLedgerDataAccess.Update(entry);
        }

        public static void Delete(int id)
        {
            IncomeLedgerDataAccess.Delete(id);
        }
    }
}
