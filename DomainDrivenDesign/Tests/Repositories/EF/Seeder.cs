using Odin.DDD;

namespace Tests.Odin.DDD.Repositories.EF;

/// <summary>
/// For easy addition of sample application data to the database.
/// </summary>
public class Seeder
{
    private readonly TestDatabaseContext _context;
    
    public Seeder(TestDatabaseContext context)
    {
        _context = context;
    }

    public Seeder Add<TAggregateRoot>(TAggregateRoot entityToAdd) 
        where TAggregateRoot : class, IAggregateRoot
    {
        _context.Set<TAggregateRoot>().Add(entityToAdd);
        return this;
    }
    
    public Seeder AddMany<TAggregateRoot>(IEnumerable<TAggregateRoot> entitiesToAdd) 
        where TAggregateRoot : class, IAggregateRoot
    {
        _context.Set<TAggregateRoot>().AddRange(entitiesToAdd);
        return this;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

}