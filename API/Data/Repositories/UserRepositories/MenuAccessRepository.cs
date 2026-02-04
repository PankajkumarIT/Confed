using API.Data.IRepository.UserRepositories;
using API.Model.Menus;
using API.Data;
using API.Data.Repositories;

namespace API.Data.Repository.UserRepositories
{
    public class MenuAccessRepository : Repository<MenuAccess>, IMenuAccessRepository
    {
        private readonly ApplicationDbContext _context;
        public MenuAccessRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }
    }
}
