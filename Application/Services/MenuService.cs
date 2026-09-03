using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class MenuService : IMenuService
    {
        private const int SuperAdminRoleId = 2;
        private readonly IUnitOfWork _unitOfWork;

        public MenuService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MenuItemDto>> GetUserMenusAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user is null || user.DeletedFlag != 1)
            {
                return [];
            }

            if (user.RoleId == SuperAdminRoleId)
            {
                var rawMenus = await _unitOfWork.Menus.GetAllActiveMenusAsync();
                return rawMenus.ToDtoList();
            }
            else
            {
                var roleId = user.RoleId ?? 0;
                var designationId = user.DesignationId ?? 0;

                var rawMenus = await _unitOfWork.Menus.GetUserMenusAsync(roleId, designationId);
                return rawMenus.ToDtoList();
            }
        }

        public async Task<List<MenuItemDto>> GetAllMenusAsync()
        {
            var rawMenus = await _unitOfWork.Menus.GetAllActiveMenusAsync();
            return rawMenus.ToDtoList();
        }
    }
}
