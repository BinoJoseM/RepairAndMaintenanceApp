using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class CategoryMasterService
    {
        public static List<CategoryMaster> GetAll(string searchText = "")
        {
            return CategoryMasterDataAccess.GetAll(searchText);
        }

        public static void Add(CategoryMaster category)
        {
            CategoryMasterDataAccess.Add(category);
        }

        public static void Update(CategoryMaster category)
        {
            CategoryMasterDataAccess.Update(category);
        }

        public static bool IsUsedInParticulars(int id)
        {
            return CategoryMasterDataAccess.IsUsedInParticulars(id);
        }

        public static void Delete(int id)
        {
            CategoryMasterDataAccess.Delete(id);
        }
    }
}
