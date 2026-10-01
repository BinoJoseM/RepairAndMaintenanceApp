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

            var sql = "SELECT e.Id, e.EntryDate, cm.CategoryName, pm.ParticularName, e.Amount, e.SourceFile, e.SourceCell FROM ExpenseLedgerEntries e LEFT JOIN CategoryMaster cm ON cm.Id = e.CategoryId LEFT JOIN ParticularMaster pm ON pm.Id = e.ParticularId WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All categories", StringComparison.OrdinalIgnoreCase))
            {
                sql += " AND cm.CategoryName = @categoryName";
            }

            if (selectedDate.HasValue)
            {
                sql += " AND date(e.EntryDate) = date(@entryDate)";
            }

            using var command = new SqliteCommand(sql, connection);

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All categories", StringComparison.OrdinalIgnoreCase))
            {
                command.Parameters.AddWithValue("@categoryName", category);
            }

            if (selectedDate.HasValue)
            {
                command.Parameters.AddWithValue("@entryDate", selectedDate.Value.ToString("yyyy-MM-dd"));
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entries.Add(new ExpenseLedgerEntries
                {
                    Id = reader.GetInt32(0),
                    EntryDate = reader.IsDBNull(1) ? null : DateTime.TryParse(reader.GetString(1), out var entryDate) ? entryDate : null,
                    Category = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Particulars = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Amount = reader.IsDBNull(4) ? 0m : Convert.ToDecimal(reader.GetValue(4)),
                    SourceFile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SourceCell = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
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

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            var categoryId = GetOrCreateCategoryId(connection, entry.Category);
            var particularId = GetOrCreateParticularId(connection, categoryId, entry.Particulars);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ExpenseLedgerEntries (EntryDate, CategoryId, ParticularId, Amount, SourceFile, SourceCell)
                VALUES (@entryDate, @categoryId, @particularId, @amount, @sourceFile, @sourceCell)
            ";

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

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            var categoryId = GetOrCreateCategoryId(connection, entry.Category);
            var particularId = GetOrCreateParticularId(connection, categoryId, entry.Particulars);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE ExpenseLedgerEntries
                SET EntryDate = @entryDate,
                    CategoryId = @categoryId,
                    ParticularId = @particularId,
                    Amount = @amount,
                    SourceFile = @sourceFile,
                    SourceCell = @sourceCell
                WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@id", entry.Id);
            command.Parameters.AddWithValue("@entryDate", entry.EntryDate.HasValue ? entry.EntryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", (object?)categoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@particularId", (object?)particularId ?? DBNull.Value);
            command.Parameters.AddWithValue("@amount", entry.Amount);
            command.Parameters.AddWithValue("@sourceFile", string.IsNullOrWhiteSpace(entry.SourceFile) ? "Manual Entry" : entry.SourceFile);
            command.Parameters.AddWithValue("@sourceCell", string.IsNullOrWhiteSpace(entry.SourceCell) ? "Manual" : entry.SourceCell);
            command.ExecuteNonQuery();
        }

        public static void Delete(int id)
        {
            if (id <= 0)
            {
                return;
            }

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ExpenseLedgerEntries WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private static long? GetOrCreateCategoryId(SqliteConnection connection, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            using (var insert = connection.CreateCommand())
            {
                insert.CommandText = "INSERT OR IGNORE INTO CategoryMaster (CategoryName, CategoryType, IsActive) VALUES (@categoryName, 'Expense', 1)";
                insert.Parameters.AddWithValue("@categoryName", categoryName.Trim());
                insert.ExecuteNonQuery();
            }

            using var lookup = connection.CreateCommand();
            lookup.CommandText = "SELECT Id FROM CategoryMaster WHERE CategoryName = @categoryName";
            lookup.Parameters.AddWithValue("@categoryName", categoryName.Trim());
            return Convert.ToInt64(lookup.ExecuteScalar());
        }

        private static long? GetOrCreateParticularId(SqliteConnection connection, long? categoryId, string particulars)
        {
            if (!categoryId.HasValue || string.IsNullOrWhiteSpace(particulars))
            {
                return null;
            }

            using (var insert = connection.CreateCommand())
            {
                insert.CommandText = "INSERT OR IGNORE INTO ParticularMaster (CategoryId, ParticularName) VALUES (@categoryId, @particularName)";
                insert.Parameters.AddWithValue("@categoryId", categoryId.Value);
                insert.Parameters.AddWithValue("@particularName", particulars.Trim());
                insert.ExecuteNonQuery();
            }

            using var lookup = connection.CreateCommand();
            lookup.CommandText = "SELECT Id FROM ParticularMaster WHERE CategoryId = @categoryId AND ParticularName = @particularName";
            lookup.Parameters.AddWithValue("@categoryId", categoryId.Value);
            lookup.Parameters.AddWithValue("@particularName", particulars.Trim());
            return Convert.ToInt64(lookup.ExecuteScalar());
        }
    }
}
