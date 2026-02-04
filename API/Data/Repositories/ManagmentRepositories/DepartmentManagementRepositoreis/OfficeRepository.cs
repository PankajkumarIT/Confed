using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Data.Repositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.DepartmentManagementRepositoreis
{
    public class OfficeRepository : Repository<Office>,IOfficeRepository
    {
        private readonly ApplicationDbContext _context;
        public OfficeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
