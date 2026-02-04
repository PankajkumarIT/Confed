using API.Data;
using API.Data.Repositories;
using API.Model.AreaModels;
using API.Data.IRepository.AreaRepositories;

namespace API.Data.Repository.AreaRepositories
{
    public class CtvRepository : Repository<Ctv>, ICtvRepository
    {
        private readonly ApplicationDbContext _context;
        public CtvRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}

