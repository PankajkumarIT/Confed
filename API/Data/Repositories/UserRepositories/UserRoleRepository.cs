
using API.Data;
using API.Data.Repositories;
using API.Model.UserModels;
using API.Data.IRepository.UserRepositories;

namespace API.Data.Repository.UserRepositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}