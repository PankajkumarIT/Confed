using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Model.ManagementModels.UserModels;

namespace API.Data.Repositories.UserRepositories
{
    public class AdministorUserRepository : Repository<Administor>, IAdministorUserRepository
    {
        private readonly ApplicationDbContext _context;
        public AdministorUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
