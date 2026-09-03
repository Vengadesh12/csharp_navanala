using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface IDesignationRepository : IRepository<Designation>
    {
        Task<List<Designation>> GetActiveDesignationsAsync();

        Task<Designation?> GetActiveDesignationByIdAsync(int id);

        Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync();
    }
}
