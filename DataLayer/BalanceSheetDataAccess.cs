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
        internal static void SaveJournalEntry(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int journalId,
            DateTime? entryDate,
            string particulars,
            decimal debit,
            decimal credit,
            string lineType,
            string sourceFile,
            int sourceRow)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = lineType.Equals("Income", StringComparison.OrdinalIgnoreCase)
                ? "SELECT ParticularId FROM IncomeLedgerEntries WHERE JournalId = @journalId"
                : "SELECT ParticularId FROM ExpenseLedgerEntries WHERE JournalId = @journalId";
            command.Parameters.AddWithValue("@journalId", journalId);
            var particularId = command.ExecuteScalar();
            command.Parameters.Clear();

            command.CommandText = @"
                UPDATE BalanceSheetItems
                SET StatementTitle = @statementTitle,
                    EntryDate = @entryDate,
                    ParticularId = @particularId,
                    Debit = @debit,
                    Credit = @credit,
                    LineType = @lineType,
                    SourceFile = @sourceFile,
                    SourceRow = @sourceRow
                WHERE JournalId = @journalId
            ";
            AddJournalEntryParameters(command, journalId, particularId, entryDate, debit, credit, lineType, sourceFile, sourceRow);
            if (command.ExecuteNonQuery() != 0)
            {
                return;
            }

            command.CommandText = @"
                INSERT INTO BalanceSheetItems
                    (JournalId, StatementTitle, EntryDate, ParticularId, Debit, Credit, LineType, SourceFile, SourceRow)
                VALUES
                    (@journalId, @statementTitle, @entryDate, @particularId, @debit, @credit, @lineType, @sourceFile, @sourceRow)
            ";
            command.ExecuteNonQuery();
        }

        private static void AddJournalEntryParameters(
            SqliteCommand command,
            int journalId,
            object? particularId,
            DateTime? entryDate,
            decimal debit,
            decimal credit,
            string lineType,
            string sourceFile,
            int sourceRow)
        {
            command.Parameters.AddWithValue("@journalId", journalId);
            command.Parameters.AddWithValue("@statementTitle", "Manual Income and Expense Entries");
            command.Parameters.AddWithValue("@entryDate", entryDate.HasValue ? entryDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : DBNull.Value);
            command.Parameters.AddWithValue("@particularId", particularId ?? DBNull.Value);
            command.Parameters.AddWithValue("@debit", debit);
            command.Parameters.AddWithValue("@credit", credit);
            command.Parameters.AddWithValue("@lineType", lineType);
            command.Parameters.AddWithValue("@sourceFile", sourceFile);
            command.Parameters.AddWithValue("@sourceRow", sourceRow);
        }

        public static List<BalanceSheetItems> GetAll(string? statementTitle = null)
        {
            var items = new List<BalanceSheetItems>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            var sql = @"
                SELECT b.Id, b.JournalId, b.ParticularId, b.StatementTitle, b.EntryDate,
                    COALESCE(pm.ParticularName, '') AS Particulars,
                    b.Debit, b.Credit, b.LineType, b.SourceFile, b.SourceRow
                FROM BalanceSheetItems b
                LEFT JOIN ParticularMaster pm ON pm.Id = b.ParticularId
                WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(statementTitle) && !string.Equals(statementTitle, "All statements", System.StringComparison.OrdinalIgnoreCase))
            {
                sql += " AND b.StatementTitle = @statementTitle";
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
                    JournalId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    ParticularId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    StatementTitle = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    EntryDate = reader.IsDBNull(4) || !DateTime.TryParse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.None, out var entryDate)
                        ? null
                        : entryDate,
                    Particulars = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Debit = reader.IsDBNull(6) ? 0m : System.Convert.ToDecimal(reader.GetValue(6)),
                    Credit = reader.IsDBNull(7) ? 0m : System.Convert.ToDecimal(reader.GetValue(7)),
                    LineType = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    SourceFile = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    SourceRow = reader.IsDBNull(10) ? 0 : reader.GetInt32(10)
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
