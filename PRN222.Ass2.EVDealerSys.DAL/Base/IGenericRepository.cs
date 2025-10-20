using System.Linq.Expressions;

namespace PRN222.Ass2.EVDealerSys.DAL.Base;
public interface IGenericRepository<T> where T : class
{
    T? GetById(int id);
    Task<T?> GetByIdAsync(int id);
    void Create(T entity);
    Task<T> CreateAsync(T entity);
    void Update(T entity);
    Task<T> UpdateAsync(T entity);
    bool Delete(T entity);
    Task<bool> DeleteAsync(T entity);
    Task<bool> DeleteAsync(int id);
    void PrepareCreate(T entity);
    void PrepareUpdate(T entity);
    void PrepareDelete(T entity);

    int Save();
    Task<int> SaveAsync();
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[]? includes);
    IEnumerable<T> GetAll(params Expression<Func<T, object>>[]? includes);
}
