using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Model.ManagementModels.UserModels;

namespace API.Data.Repositories.ManagmentRepositories.UserManagementRepositories
{
    public class TransportUserRepository : Repository<TransporterUser>, ITransportUserRepository
    {
        private readonly ApplicationDbContext _context;

        public TransportUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }

}
