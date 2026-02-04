using API.Data.IRepositories.IManagmentRepositories;
using API.Model.ManagementModels;

namespace API.Data.Repositories.ManagmentRepositories
{
    public class DepartmentBankOrganizationMapRepository : Repository<DepartmentBankOrganizationMap>, IDepartmentBankOrganizationMapRepository
    {
        private readonly ApplicationDbContext _context;
        public DepartmentBankOrganizationMapRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
