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
            SaveManualEntry(entry);
        }

        public static void SaveManualEntry(IncomeLedgerEntries entry)
        {
            IncomeLedgerDataAccess.SaveManualEntry(entry);
        }

        public static void Update(IncomeLedgerEntries entry)
        {
            UpdateManualEntry(entry);
        }

        public static void UpdateManualEntry(IncomeLedgerEntries entry)
        {
            IncomeLedgerDataAccess.UpdateManualEntry(entry);
        }

        public static void Delete(int id)
        {
            IncomeLedgerDataAccess.Delete(id);
        }
    }
}
