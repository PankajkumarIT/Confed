using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Data.Repositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.DepartmentManagementRepositoreis
{
    public class DesignationRepository : Repository<Designation>,IDesignationRepository
    {
        private readonly ApplicationDbContext _context;
        public DesignationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }

}
