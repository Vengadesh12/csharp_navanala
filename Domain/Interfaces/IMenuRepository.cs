using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<Menu>> GetAllActiveMenusAsync();

        Task<List<Menu>> GetUserMenusAsync(int roleId, int designationId);
    }
}
