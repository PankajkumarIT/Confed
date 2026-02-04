using API.Data.IRepository.UserRepositories;
using API.Model.Menus;
using API.Data;
using API.Data.Repositories;

namespace API.Data.Repository.UserRepositories
{
    public class MenuRepository : Repository<Menu>, IMenuRepository
    {
        private readonly ApplicationDbContext _context;

        public MenuRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public object GetMenuWithSubmenus(Menu Menu, List<Menu> AllMenus)
        {
            return new
            {
                Id = Menu.Id,
                MenuCode = Menu.MenuCode,
                MenuName = Menu.MenuName,
                Path = Menu.Path,
                Icon = Menu.Icon,
                Status = Menu.Status,
                ParentCode = Menu.ParentCode,
                SubMenus = AllMenus
                    .Where(subMenu => subMenu.ParentCode == Menu.MenuCode)
                    .Select(subMenu => GetMenuWithSubmenus(subMenu, AllMenus))
                    .ToList()
            };
        }
    }
}
