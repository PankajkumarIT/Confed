using System.Linq.Expressions;

namespace API.Data.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(
           Expression<Func<T, bool>> filter = null,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           string includeProperties = null);


        Task<T> FirstOrDefaultAsync(
          Expression<Func<T, bool>> filter = null,
          string includeProperties = null);
        Task<T> GetByIntAsync(int code);


        Task AddAsync(T entity);

        Task<T> GetAsync(string code);
        Task UpdateAsync(string entityCode, Func<T, Task> updateAction);

        Task UpdateByIntAsync(int entityCode, Func<T, Task> updateAction);
        Task DeleteAsync(int id);

        Task RemoveAsync(T entity);

        Task RemoveAsync(string code);

        Task RemoveByIntAsync(int code);

        Task RemoveRangeAsync(IEnumerable<T> values);

        IFormFile DecrypteIFromFileWithFileNameBase64(string value,string filename);
        IFormFile DecrypteIFromFileBase64(string value);

        string DecrypteBase64(string value);

        string GenrateUniqueCode();

        Task<string> GetFileAsBase64Async(string filePath);

        Task<string> SaveReportFile(string documentionFile);
        Task<string> GenerateFileNumberAsync();
    }
}
