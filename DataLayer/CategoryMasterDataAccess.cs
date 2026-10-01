using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.DataLayer
{
    public class CategoryMasterDataAccess
    {
        public static List<CategoryMaster> GetAll(string searchText = "")
        {
            var categories = new List<CategoryMaster>();
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, CategoryName, CategoryType, IsActive, CreatedAt, UpdatedAt
                FROM CategoryMaster
                WHERE CategoryName LIKE @search
                ORDER BY CategoryName
            ";
            command.Parameters.AddWithValue("@search", $"%{searchText}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new CategoryMaster
                {
                    Id = reader.GetInt32(0),
                    CategoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    CategoryType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = !reader.IsDBNull(3) && reader.GetInt32(3) == 1,
                    CreatedAt = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    UpdatedAt = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return categories;
        }

        public static void Add(CategoryMaster category)
        {
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO CategoryMaster (CategoryName, CategoryType, IsActive) VALUES (@name, @type, @active)";
            command.Parameters.AddWithValue("@name", category.CategoryName.Trim());
            command.Parameters.AddWithValue("@type", category.CategoryType);
            command.Parameters.AddWithValue("@active", category.IsActive ? 1 : 0);
            command.ExecuteNonQuery();
        }

        public static void Update(CategoryMaster category)
        {
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE CategoryMaster SET CategoryName = @name, CategoryType = @type, IsActive = @active, UpdatedAt = datetime('now') WHERE Id = @id";
            command.Parameters.AddWithValue("@id", category.Id);
            command.Parameters.AddWithValue("@name", category.CategoryName.Trim());
            command.Parameters.AddWithValue("@type", category.CategoryType);
            command.Parameters.AddWithValue("@active", category.IsActive ? 1 : 0);
            command.ExecuteNonQuery();
        }

        public static void Delete(int id)
        {
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM CategoryMaster WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
    }
}
