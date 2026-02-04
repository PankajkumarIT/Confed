using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Data.Repositories.ManagmentRepositories.DepartmentManagementRepositoreis
{
    public class UploadFileInfoHistoryRepository : Repository<UploadFileInfoHistory>, IUploadFileInfoHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        public UploadFileInfoHistoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }

}
