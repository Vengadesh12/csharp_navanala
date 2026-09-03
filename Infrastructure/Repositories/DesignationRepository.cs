using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class DesignationRepository : Repository<Designation>, IDesignationRepository
    {
        public DesignationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Designation>> GetActiveDesignationsAsync()
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(d => d.DeletedFlag == 1)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Designation?> GetActiveDesignationByIdAsync(int id)
        {
            return await _context.Designations
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedFlag == 1);
        }

        public async Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync()
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(d => d.DeletedFlag == 1)
                .ToDictionaryAsync(d => d.Id, d => d.Name);
        }
    }
}
