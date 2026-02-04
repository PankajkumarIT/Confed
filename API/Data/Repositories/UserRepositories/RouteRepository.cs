using API.Data;
using API.Data.Repositories;
using API.Data.IRepository.UserRepositories;
using Route = API.Model.Routes.Route;

namespace API.Data.Repository.UserRepositories
{
    public class RouteRepository : Repository<Route>, IRouteRepository
    {
        private readonly ApplicationDbContext _context;

        public RouteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }

        public object GetRouteWithSubRoutes(Route Route, List<Route> AllRoutes)
        {
            return new
            {
                Id = Route.Id,
                RouteCode = Route.RouteCode,
                RouteName = Route.RouteName,
                Path = Route.Path,
                Status = Route.Status,
                ParentCode = Route.ParentCode,
                SubRoutes = AllRoutes
                    .Where(subRoute => subRoute.ParentCode == Route.RouteCode)
                    .Select(subRoute => GetRouteWithSubRoutes(subRoute, AllRoutes))
                    .ToList()
            };
        }
    }
}
