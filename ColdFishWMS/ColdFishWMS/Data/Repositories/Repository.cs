using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(DbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Set.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await Set.FindAsync(id);
    }

    public async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
    {
        return await Set.Where(predicate).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await Set.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        Set.Remove(entity);
        await Context.SaveChangesAsync();
    }
}





