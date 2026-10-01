using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class BalanceSheetService
    {
        public static List<BalanceSheetItems> GetAll(string? statementTitle = null)
        {
            return BalanceSheetDataAccess.GetAll(statementTitle);
        }
    }
}
