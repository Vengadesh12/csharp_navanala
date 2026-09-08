using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<MenuModel>> GetAllActiveMenusAsync();
 
        Task<List<string>> GetAllActiveMenuNamesAsync();

        Task<List<MenuModel>> GetUserMenusAsync(int roleId, int designationId, int? userId = null);

        Task<List<string>> GetUserMenuNamesAsync(int roleId, int designationId, int? userId = null);
    }
}
