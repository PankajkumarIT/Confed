using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Data.Repositories;
using API.Model.ManagementModels.UserModels;

namespace API.Data.Repositories.ManagmentRepositories.UserManagementRepositories
{
    public class BankUserRepository : Repository<BankUser>,IBankUserRepository
    {
        private readonly ApplicationDbContext _context;
        public BankUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
