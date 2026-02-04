using API.Data;
using API.Data.Repositories;
using API.Model.AreaModels;
using API.Data.IRepository.AreaRepositories;

namespace API.Data.Repository.AreaRepositories
{
    public class DistrictRepository : Repository<District>, IDistrictRepository
    {
        private readonly ApplicationDbContext _context;
        public DistrictRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}