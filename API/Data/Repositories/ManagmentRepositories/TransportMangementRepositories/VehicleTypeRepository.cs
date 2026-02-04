using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.TransporterManagement;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class VehicleTypeRepository : Repository<VehicleType>, IVehicleTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
