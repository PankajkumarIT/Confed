using API.Data.IRepositories.IManagmentRepositories;
using API.Model.ManagementModels;

namespace API.Data.Repositories.ManagmentRepositories
{
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        private readonly ApplicationDbContext _context;

        public OrganizationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
