using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Model.ManagementModels.TransporterManagement;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories
{
    public class VehicleDriverMapRepository :Repository<VehicleDriverMap>, IVehicleDriverMapRepository
    {
        private readonly ApplicationDbContext _context;

    public VehicleDriverMapRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}
}
