using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.DataLayer
{
    public class BalanceSheetDataAccess
    {
        public static List<BalanceSheetItems> GetAll(string? statementTitle = null)
        {
            var items = new List<BalanceSheetItems>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            var sql = "SELECT Id, StatementTitle, Particulars, Debit, Credit, LineType, SourceFile, SourceRow FROM BalanceSheetItems WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(statementTitle) && !string.Equals(statementTitle, "All statements", System.StringComparison.OrdinalIgnoreCase))
            {
                sql += " AND StatementTitle = @statementTitle";
            }

            using var command = new SqliteCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(statementTitle) && !string.Equals(statementTitle, "All statements", System.StringComparison.OrdinalIgnoreCase))
            {
                command.Parameters.AddWithValue("@statementTitle", statementTitle);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new BalanceSheetItems
                {
                    Id = reader.GetInt32(0),
                    StatementTitle = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Particulars = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Debit = reader.IsDBNull(3) ? 0m : System.Convert.ToDecimal(reader.GetValue(3)),
                    Credit = reader.IsDBNull(4) ? 0m : System.Convert.ToDecimal(reader.GetValue(4)),
                    LineType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SourceFile = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    SourceRow = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                });
            }

            return items;
        }
    }
}
