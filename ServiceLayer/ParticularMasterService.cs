using System.Collections.Generic;
using RepairAndMaintenanceApp.DataLayer;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.ServiceLayer
{
    public class ParticularMasterService
    {
        public static List<ParticularMaster> GetAll(string searchText = "")
        {
            return ParticularMasterDataAccess.GetAll(searchText);
        }

        public static List<CategoryMaster> GetActiveCategories()
        {
            return ParticularMasterDataAccess.GetActiveCategories();
        }

        public static void Add(ParticularMaster particular)
        {
            ParticularMasterDataAccess.Add(particular);
        }

        public static void Update(ParticularMaster particular)
        {
            ParticularMasterDataAccess.Update(particular);
        }

        public static void Delete(int id)
        {
            ParticularMasterDataAccess.Delete(id);
        }
    }
}
