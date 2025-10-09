using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN222.Ass2.EVDealerSys.Repositories.Base;
public interface IGenericRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    Task<IEnumerable<T>> GetAllAsync();
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
}