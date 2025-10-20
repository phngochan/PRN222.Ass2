using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using PRN222.Ass2.EVDealerSys.DAL.Context;

namespace PRN222.Ass2.EVDealerSys.DAL.Base;
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly EvdealerDbContext _context;
    public GenericRepository(EvdealerDbContext context) => _context = context;

    public virtual IEnumerable<T> GetAll(params Expression<Func<T, object>>[]? includes)
    {
        IQueryable<T> query = _context.Set<T>();

        // Apply includes
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return query.AsNoTracking().ToList();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[]? includes)
    {
        IQueryable<T> query = _context.Set<T>();

        // Apply includes
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return await query.AsNoTracking().ToListAsync();
    }

    public virtual T? GetById(int id)
    {
        var entity = _context.Set<T>().Find(id);
        if (entity != null)
            _context.Entry(entity).State = EntityState.Detached;
        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity != null)
            _context.Entry(entity).State = EntityState.Detached;
        return entity;
    }
    public virtual void Create(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public virtual void Update(T entity)
    {
        var local = _context.Set<T>().Local.FirstOrDefault(e => e == entity);
        if (local != null)
            _context.Entry(local).State = EntityState.Detached;

        _context.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        _context.SaveChanges();
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        var local = _context.Set<T>().Local.FirstOrDefault(e => e == entity);
        if (local != null)
            _context.Entry(local).State = EntityState.Detached;

        _context.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return entity;
    }
    public virtual bool Delete(T entity)
    {
        if (entity == null) return false;
        _context.Set<T>().Remove(entity);
        _context.SaveChanges();
        return true;
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        if (entity == null) return false;
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    #region Separating asigned entity and save operators        

    public void PrepareCreate(T entity)
    {
        _context.Add(entity);
    }

    public void PrepareUpdate(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
    }

    public void PrepareDelete(T entity)
    {
        _context.Remove(entity);
    }

    public int Save()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    #endregion Separating asign entity and save operators

}
