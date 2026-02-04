using API.Data.IRepository.UserRepositories;
using API.Model.Routes;
using API.Data.Repositories;
using API.Data;

namespace API.Data.Repository.UserRepositories
{
    public class RouteAccessRepository:Repository<RouteAccess>,IRouteAccessRepository
    {
        private readonly ApplicationDbContext _context;

        public RouteAccessRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }
    }
}
