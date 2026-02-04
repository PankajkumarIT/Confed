using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.TransporterManagement;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
