using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.DataLayer
{
    public class ExpenseLedgerDataAccess
    {
        public static List<ExpenseLedgerEntries> GetAll(string? category = null, DateTime? selectedDate = null)
        {
            var entries = new List<ExpenseLedgerEntries>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            var sql = "SELECT e.Id, e.JournalId, e.EntryDate, cm.CategoryName, pm.ParticularName, e.Amount, e.SourceFile, e.SourceCell FROM ExpenseLedgerEntries e LEFT JOIN CategoryMaster cm ON cm.Id = e.CategoryId LEFT JOIN ParticularMaster pm ON pm.Id = e.ParticularId WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All categories", StringComparison.OrdinalIgnoreCase))
            {
                sql += " AND cm.CategoryName = @categoryName";
            }

            if (selectedDate.HasValue)
            {
                sql += " AND date(e.EntryDate) >= date(@monthStart) AND date(e.EntryDate) < date(@nextMonthStart)";
            }

            using var command = new SqliteCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All categories", StringComparison.OrdinalIgnoreCase))
            {
                command.Parameters.AddWithValue("@categoryName", category);
            }

            if (selectedDate.HasValue)
            {
                var monthStart = new DateTime(selectedDate.Value.Year, selectedDate.Value.Month, 1);
                command.Parameters.AddWithValue("@monthStart", monthStart.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@nextMonthStart", monthStart.AddMonths(1).ToString("yyyy-MM-dd"));
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entries.Add(new ExpenseLedgerEntries
                {
                    Id = reader.GetInt32(0),
                    JournalId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    EntryDate = reader.IsDBNull(2) ? null : DateTime.TryParse(reader.GetString(2), out var entryDate) ? entryDate : null,
                    Category = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Particulars = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Amount = reader.IsDBNull(5) ? 0m : Convert.ToDecimal(reader.GetValue(5)),
                    SourceFile = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    SourceCell = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                });
            }

            return entries;
        }

        public static void Add(ExpenseLedgerEntries entry)
        {
            if (entry == null)
            {
                return;
            }

            SaveManualEntry(entry);
        }

        public static void SaveManualEntry(ExpenseLedgerEntries entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var categoryId = GetOrCreateCategoryId(connection, entry.Category, transaction);
            var particularId = GetOrCreateParticularId(connection, categoryId, entry.Particulars, transaction);

            var journalEntry = JournalTransactionDataAccess.AddFromLedger(
                connection,
                transaction,
                entry.EntryDate,
                categoryId,
                particularId,
                0m,
                entry.Amount,
                string.IsNullOrWhiteSpace(entry.SourceFile) ? "Manual Entry" : entry.SourceFile);
            entry.JournalId = journalEntry.JournalId;
            entry.SourceCell = $"Manual-{journalEntry.JournalId}";

            Add(entry, connection, transaction);
            BalanceSheetDataAccess.SaveJournalEntry(
                connection,
                transaction,
                journalEntry.JournalId,
                entry.EntryDate,
                entry.Particulars,
                0m,
                entry.Amount,
                "Expense",
                journalEntry.SourceFile,
                journalEntry.SourceRow);

            transaction.Commit();
        }

        public static void UpdateManualEntry(ExpenseLedgerEntries entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            if (entry.Id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entry), "An existing expense entry ID is required.");
            }

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var categoryId = GetOrCreateCategoryId(connection, entry.Category, transaction);
            var particularId = GetOrCreateParticularId(connection, categoryId, entry.Particulars, transaction);

            var journalEntry = entry.JournalId.HasValue
                ? JournalTransactionDataAccess.UpdateFromLedger(connection, transaction, entry.JournalId.Value, entry.EntryDate, categoryId, particularId, 0m, entry.Amount)
                : JournalTransactionDataAccess.AddFromLedger(
                    connection,
                    transaction,
                    entry.EntryDate,
                    categoryId,
                    particularId,
                    0m,
                    entry.Amount,
                    string.IsNullOrWhiteSpace(entry.SourceFile) ? "Manual Entry" : entry.SourceFile);
            entry.JournalId = journalEntry.JournalId;

            if (Update(entry, connection, transaction) == 0)
            {
                throw new InvalidOperationException($"Expense entry {entry.Id} was not found.");
            }

            BalanceSheetDataAccess.SaveJournalEntry(
                connection,
                transaction,
                journalEntry.JournalId,
                entry.EntryDate,
                entry.Particulars,
                0m,
                entry.Amount,
                "Expense",
                journalEntry.SourceFile,
                journalEntry.SourceRow);

            transaction.Commit();
        }

        private static void Add(ExpenseLedgerEntries entry, SqliteConnection connection, SqliteTransaction? transaction)
        {
            var categoryId = GetOrCreateCategoryId(connection, entry.Category, transaction);
            var particularId = GetOrCreateParticularId(connection, categoryId, entry.Particulars, transaction);

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT INTO ExpenseLedgerEntries (JournalId, EntryDate, CategoryId, ParticularId, Amount, SourceFile, SourceCell)
                VALUES (@journalId, @entryDate, @categoryId, @particularId, @amount, @sourceFile, @sourceCell)
            ";

            command.Parameters.AddWithValue("@journalId", (object?)entry.JournalId ?? DBNull.Value);
            command.Parameters.AddWithValue("@entryDate", entry.EntryDate.HasValue ? entry.EntryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", (object?)categoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@particularId", (object?)particularId ?? DBNull.Value);
            command.Parameters.AddWithValue("@amount", entry.Amount);
            command.Parameters.AddWithValue("@sourceFile", string.IsNullOrWhiteSpace(entry.SourceFile) ? "Manual Entry" : entry.SourceFile);
            command.Parameters.AddWithValue("@sourceCell", string.IsNullOrWhiteSpace(entry.SourceCell) ? "Manual" : entry.SourceCell);
            command.ExecuteNonQuery();
        }

        public static void Update(ExpenseLedgerEntries entry)
        {
            if (entry == null || entry.Id <= 0)
            {
                return;
            }

            UpdateManualEntry(entry);
        }

        private static int Update(ExpenseLedgerEntries entry, SqliteConnection connection, SqliteTransaction? transaction)
        {
            var categoryId = GetOrCreateCategoryId(connection, entry.Category, transaction);
            var particularId = GetOrCreateParticularId(connection, categoryId, entry.Particulars, transaction);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE ExpenseLedgerEntries
                SET JournalId = @journalId,
                    EntryDate = @entryDate,
                    CategoryId = @categoryId,
                    ParticularId = @particularId,
                    Amount = @amount,
                    SourceFile = @sourceFile,
                    SourceCell = @sourceCell
                WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@id", entry.Id);
            command.Parameters.AddWithValue("@journalId", (object?)entry.JournalId ?? DBNull.Value);
            command.Parameters.AddWithValue("@entryDate", entry.EntryDate.HasValue ? entry.EntryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", (object?)categoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@particularId", (object?)particularId ?? DBNull.Value);
            command.Parameters.AddWithValue("@amount", entry.Amount);
            command.Parameters.AddWithValue("@sourceFile", string.IsNullOrWhiteSpace(entry.SourceFile) ? "Manual Entry" : entry.SourceFile);
            command.Parameters.AddWithValue("@sourceCell", string.IsNullOrWhiteSpace(entry.SourceCell) ? "Manual" : entry.SourceCell);
            return command.ExecuteNonQuery();
        }

        public static void Delete(int id)
        {
            if (id <= 0)
            {
                return;
            }

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();

            long? journalId = null;
            using (var lookup = connection.CreateCommand())
            {
                lookup.Transaction = transaction;
                lookup.CommandText = "SELECT JournalId FROM ExpenseLedgerEntries WHERE Id = @id";
                lookup.Parameters.AddWithValue("@id", id);
                var result = lookup.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    journalId = Convert.ToInt64(result);
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "DELETE FROM ExpenseLedgerEntries WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }

            if (journalId.HasValue)
            {
                using var cleanup = connection.CreateCommand();
                cleanup.Transaction = transaction;
                cleanup.CommandText = @"
                    DELETE FROM BalanceSheetItems WHERE JournalId = @journalId;
                    DELETE FROM JournalTransactions
                    WHERE Id = @journalId
                      AND NOT EXISTS (SELECT 1 FROM ExpenseLedgerEntries WHERE JournalId = @journalId)
                      AND NOT EXISTS (SELECT 1 FROM IncomeLedgerEntries WHERE JournalId = @journalId);
                ";
                cleanup.Parameters.AddWithValue("@journalId", journalId.Value);
                cleanup.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static long? GetOrCreateCategoryId(SqliteConnection connection, string categoryName, SqliteTransaction? transaction = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = "INSERT OR IGNORE INTO CategoryMaster (CategoryName, CategoryType, IsActive) VALUES (@categoryName, 'Expense', 1)";
                insert.Parameters.AddWithValue("@categoryName", categoryName.Trim());
                insert.ExecuteNonQuery();
            }

            using var lookup = connection.CreateCommand();
            lookup.Transaction = transaction;
            lookup.CommandText = "SELECT Id FROM CategoryMaster WHERE CategoryName = @categoryName";
            lookup.Parameters.AddWithValue("@categoryName", categoryName.Trim());
            return Convert.ToInt64(lookup.ExecuteScalar());
        }

        private static long? GetOrCreateParticularId(SqliteConnection connection, long? categoryId, string particulars, SqliteTransaction? transaction = null)
        {
            if (!categoryId.HasValue || string.IsNullOrWhiteSpace(particulars))
            {
                return null;
            }

            using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = "INSERT OR IGNORE INTO ParticularMaster (CategoryId, ParticularName) VALUES (@categoryId, @particularName)";
                insert.Parameters.AddWithValue("@categoryId", categoryId.Value);
                insert.Parameters.AddWithValue("@particularName", particulars.Trim());
                insert.ExecuteNonQuery();
            }

            using var lookup = connection.CreateCommand();
            lookup.Transaction = transaction;
            lookup.CommandText = "SELECT Id FROM ParticularMaster WHERE CategoryId = @categoryId AND ParticularName = @particularName";
            lookup.Parameters.AddWithValue("@categoryId", categoryId.Value);
            lookup.Parameters.AddWithValue("@particularName", particulars.Trim());
            return Convert.ToInt64(lookup.ExecuteScalar());
        }
    }
}
