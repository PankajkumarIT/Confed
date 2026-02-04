using API.Data.IRepositories;
using API.Model.UserModels;

namespace API.Data.IRepository.UserRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<bool> IsUniqueUser(string MobileNumber);

        public Task<bool> RegisterUser(User User);

        public Task<string> Authenticate(string MobileNumber, string RoleId);

        public Task<bool> ActiveDeactiveUser(string UserCode);

    }
}
