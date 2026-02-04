
using API.Data;
using API.Data.Repositories;
using API.Model.UserModels;
using API.Data.IRepository.UserRepositories;

namespace API.Data.Repository.UserRepositories
{
    public class RoleAccessRepository : Repository<RoleAccess>, IRoleAccessRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleAccessRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }
    }
}
