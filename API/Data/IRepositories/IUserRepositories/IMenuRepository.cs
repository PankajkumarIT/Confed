using API.Data.IRepositories;
using API.Model.Menus;

namespace API.Data.IRepository.UserRepositories
{
    public interface IMenuRepository : IRepository<Menu>
    {
        public object GetMenuWithSubmenus(Menu Menu, List<Menu> AllMenus);
    }

}
