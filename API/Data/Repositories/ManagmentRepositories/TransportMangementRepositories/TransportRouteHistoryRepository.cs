using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{

    public class TransportRouteHistoryRepository : Repository<TransportRouteHistory>, ITransportRouteHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        public TransportRouteHistoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
