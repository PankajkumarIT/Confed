using API.Data.IRepositories.IManagmentRepositories;
using API.Model.ManagementModels;

namespace API.Data.Repositories.ManagmentRepositories
{
    public class UserFileDirectoryRepository : Repository<UserFileDirectory>, IUserFileDirectoryRepository
    {
        private readonly ApplicationDbContext _context;
        public UserFileDirectoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
