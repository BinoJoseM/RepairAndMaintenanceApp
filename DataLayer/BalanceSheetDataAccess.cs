using System;
using System.Collections.Generic;
using System.Globalization;
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

            var sql = "SELECT Id, StatementTitle, EntryDate, Particulars, Debit, Credit, LineType, SourceFile, SourceRow FROM BalanceSheetItems WHERE 1 = 1";
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
                    EntryDate = reader.IsDBNull(2) || !DateTime.TryParse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.None, out var entryDate)
                        ? null
                        : entryDate,
                    Particulars = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Debit = reader.IsDBNull(4) ? 0m : System.Convert.ToDecimal(reader.GetValue(4)),
                    Credit = reader.IsDBNull(5) ? 0m : System.Convert.ToDecimal(reader.GetValue(5)),
                    LineType = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    SourceFile = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    SourceRow = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                });
            }

            return items;
        }

        public static List<BalanceSheetItems> GetMonthlyIncomeExpenseSummary(DateTime selectedMonth)
        {
            var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);
            var statementTitle = $"Monthly Income and Expense Summary - {monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}";
            var sourceFile = $"Generated monthly summary ({monthStart:yyyy-MM})";
            var items = new List<BalanceSheetItems>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT EntryType, Category, Particulars, SUM(Amount) AS Amount
                FROM (
                    SELECT
                        'Expense' AS EntryType,
                        COALESCE(cm.CategoryName, '') AS Category,
                        COALESCE(pm.ParticularName, '') AS Particulars,
                        e.Amount
                    FROM ExpenseLedgerEntries e
                    LEFT JOIN CategoryMaster cm ON cm.Id = e.CategoryId
                    LEFT JOIN ParticularMaster pm ON pm.Id = e.ParticularId
                    WHERE (date(e.EntryDate) >= date(@monthStart) AND date(e.EntryDate) < date(@nextMonthStart))
                        OR (e.EntryDate IS NULL AND e.SourceFile LIKE @sourceFilePattern)

                    UNION ALL

                    SELECT
                        'Income' AS EntryType,
                        COALESCE(cm.CategoryName, '') AS Category,
                        COALESCE(pm.ParticularName, '') AS Particulars,
                        e.Amount
                    FROM IncomeLedgerEntries e
                    LEFT JOIN CategoryMaster cm ON cm.Id = e.CategoryId
                    LEFT JOIN ParticularMaster pm ON pm.Id = e.ParticularId
                    WHERE (date(e.EntryDate) >= date(@monthStart) AND date(e.EntryDate) < date(@nextMonthStart))
                        OR (e.EntryDate IS NULL AND e.SourceFile LIKE @sourceFilePattern)
                )
                GROUP BY EntryType, Category, Particulars
                ORDER BY CASE EntryType WHEN 'Income' THEN 0 ELSE 1 END, Category, Particulars
            ";
            command.Parameters.AddWithValue("@monthStart", monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@nextMonthStart", nextMonthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@sourceFilePattern", $"%{monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}.xlsx");

            var incomeTotal = 0m;
            var expenseTotal = 0m;
            var sourceRow = 0;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var entryType = reader.GetString(0);
                    var category = reader.GetString(1);
                    var particulars = reader.GetString(2);
                    var amount = Convert.ToDecimal(reader.GetValue(3));
                    var isIncome = string.Equals(entryType, "Income", StringComparison.Ordinal);

                    if (isIncome)
                    {
                        incomeTotal += amount;
                    }
                    else
                    {
                        expenseTotal += amount;
                    }

                    var description = string.IsNullOrWhiteSpace(category)
                        ? particulars
                        : string.IsNullOrWhiteSpace(particulars) ? category : $"{category} - {particulars}";

                    items.Add(new BalanceSheetItems
                    {
                        StatementTitle = statementTitle,
                        Particulars = description,
                        Debit = isIncome ? 0m : amount,
                        Credit = isIncome ? amount : 0m,
                        LineType = entryType,
                        SourceFile = sourceFile,
                        SourceRow = ++sourceRow
                    });
                }
            }

            items.Add(new BalanceSheetItems
            {
                StatementTitle = statementTitle,
                Particulars = "Total Income",
                Credit = incomeTotal,
                LineType = "Total",
                SourceFile = sourceFile,
                SourceRow = ++sourceRow
            });
            items.Add(new BalanceSheetItems
            {
                StatementTitle = statementTitle,
                Particulars = "Total Expenses",
                Debit = expenseTotal,
                LineType = "Total",
                SourceFile = sourceFile,
                SourceRow = ++sourceRow
            });

            return items;
        }
    }
}
