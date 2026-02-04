using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Model.ManagementModels.UserModels;

namespace API.Data.Repositories.ManagmentRepositories.UserManagementRepositories
{
    public class DepartmentUserRepository : Repository<DepartmentUser>, IDepartmentUserRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
