using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Contracts;

namespace MyBackend.Application.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuItemDto>> GetUserMenusAsync(int userId);
        Task<List<MenuItemDto>> GetAllMenusAsync();
    }
}
