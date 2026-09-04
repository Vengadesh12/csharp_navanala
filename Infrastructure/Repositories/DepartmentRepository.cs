using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Department>> GetActiveDepartmentsWithDesignationsAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.DeletedFlag == 1)
                .Include(d => d.Designations.Where(des => des.DeletedFlag == 1))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department?> GetActiveDepartmentByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Designations.Where(des => des.DeletedFlag == 1))
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedFlag == 1);
        }

        public async Task<Dictionary<int, string>> GetDepartmentNameDictionaryAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.DeletedFlag == 1)
                .ToDictionaryAsync(d => d.Id, d => d.Name);
        }

        public async Task<bool> DepartmentExistsByNameAsync(string name, int? excludeId = null)
        {
            var trimmed = name.Trim().ToLower();
            return await _context.Departments
                .AnyAsync(d => d.DeletedFlag == 1 &&
                               (!excludeId.HasValue || d.Id != excludeId.Value) &&
                               d.Name.ToLower() == trimmed);
        }
    }
}
