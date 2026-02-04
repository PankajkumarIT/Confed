using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.TransporterManagement;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class UnitMasterRepository : Repository<UnitMaster>, IUnitMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public UnitMasterRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}

