using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.DepartmentManagementRepositoreis
{
    public class CommodityMasterRepository : Repository<CommodityMaster>, ICommodityMasterRepository
    {
        private readonly ApplicationDbContext _context;
        public CommodityMasterRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
