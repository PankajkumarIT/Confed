using API.Data.IRepositories.IManagmentRepositories.IBankManagementRepositories;
using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Data.Repositories;
using API.Model.ManagementModels.BankManagement;

namespace API.Data.Repositories.ManagmentRepositories.BankManagementRepositories
{
    public class BankBranchRepository : Repository<BankBranch>, IBankBranchRepository
    {
        private readonly ApplicationDbContext _context;
        public BankBranchRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
