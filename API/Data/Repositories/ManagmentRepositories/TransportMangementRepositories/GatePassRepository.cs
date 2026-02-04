using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class GatePassRepository : Repository<GatePass>, IGatePassRepository
    {
        private readonly ApplicationDbContext _context;
        public GatePassRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
