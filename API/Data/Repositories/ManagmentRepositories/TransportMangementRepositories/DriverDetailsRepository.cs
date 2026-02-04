using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.TransporterManagement;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class DriverDetailsRepository : Repository<DriverDetails>,IDriverDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public DriverDetailsRepository(ApplicationDbContext context) :base(context) 
        {
            _context = context;
        }
    }
}