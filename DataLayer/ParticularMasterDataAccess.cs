using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.DataLayer
{
    public class ParticularMasterDataAccess
    {
        public static List<ParticularMaster> GetAll(string searchText = "")
        {
            var particulars = new List<ParticularMaster>();
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.CategoryId, c.CategoryName, p.ParticularName, p.CreatedAt
                FROM ParticularMaster p
                JOIN CategoryMaster c ON c.Id = p.CategoryId
                WHERE p.ParticularName LIKE @search OR c.CategoryName LIKE @search
                ORDER BY c.CategoryName, p.ParticularName
            ";
            command.Parameters.AddWithValue("@search", $"%{searchText}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                particulars.Add(new ParticularMaster
                {
                    Id = reader.GetInt32(0),
                    CategoryId = reader.GetInt32(1),
                    CategoryName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ParticularName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    CreatedAt = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                });
            }

            return particulars;
        }

        public static List<CategoryMaster> GetActiveCategories()
        {
            var categories = new List<CategoryMaster>();
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, CategoryName FROM CategoryMaster WHERE IsActive = 1 ORDER BY CategoryName";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new CategoryMaster
                {
                    Id = reader.GetInt32(0),
                    CategoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }

            return categories;
        }

        public static List<string> GetNamesByCategory(string categoryName)
        {
            var names = new List<string>();
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.ParticularName
                FROM ParticularMaster p
                JOIN CategoryMaster c ON c.Id = p.CategoryId
                WHERE c.CategoryName = @categoryName AND c.IsActive = 1
                ORDER BY p.ParticularName
            ";
            command.Parameters.AddWithValue("@categoryName", categoryName);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                names.Add(reader.GetString(0));
            }

            return names;
        }

        public static void Add(ParticularMaster particular)
        {
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO ParticularMaster (CategoryId, ParticularName) VALUES (@categoryId, @name)";
            command.Parameters.AddWithValue("@categoryId", particular.CategoryId);
            command.Parameters.AddWithValue("@name", particular.ParticularName.Trim());
            command.ExecuteNonQuery();
        }

        public static void Update(ParticularMaster particular)
        {
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE ParticularMaster SET CategoryId = @categoryId, ParticularName = @name WHERE Id = @id";
            command.Parameters.AddWithValue("@id", particular.Id);
            command.Parameters.AddWithValue("@categoryId", particular.CategoryId);
            command.Parameters.AddWithValue("@name", particular.ParticularName.Trim());
            command.ExecuteNonQuery();
        }

        public static void Delete(int id)
        {
            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ParticularMaster WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
    }
}
