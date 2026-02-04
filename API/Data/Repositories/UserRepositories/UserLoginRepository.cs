using API.Data;
using API.Data.Repositories;
using API.Model.UserModels;
using API.Data.IRepository.UserRepositories;

namespace API.Data.Repository.UserRepositories
{
    public class UserLoginRepository : Repository<UserLogin>, IUserLoginRepository
    {
        private readonly ApplicationDbContext _context;

        public UserLoginRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
