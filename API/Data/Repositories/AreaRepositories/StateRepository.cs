using API.Data;
using API.Data.Repositories;
using API.Model.AreaModels;
using API.Data.IRepository.AreaRepositories;

namespace API.Data.Repository.AreaRepositories
{
    public class StateRepository : Repository<State>, IStateRepository
    {
        private readonly ApplicationDbContext _context;
        public StateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }
    }
}
