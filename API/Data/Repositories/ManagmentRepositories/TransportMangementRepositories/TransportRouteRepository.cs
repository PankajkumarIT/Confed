using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class TransportRouteRepository : Repository<TransportRoute>, ITransportRouteRepository
    {
        private readonly ApplicationDbContext _context;

        public TransportRouteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}