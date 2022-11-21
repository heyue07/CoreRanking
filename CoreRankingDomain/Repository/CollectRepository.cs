namespace CoreRankingDomain.Repository;

public class CollectRepository : ICollectRepository
{
    private readonly CollectDbContext _context;

    public CollectRepository(CollectDbContext context)
    {
        _context = context;
    }

    public async Task AddByModel(Collect collect)
    {
        _context.Collect.Add(collect);
    }

    public async Task<List<Collect>> GetCollectsByRole(int roleId)
    {
        return await _context.Collect.Where(x => x.RoleId.Equals(roleId)).ToListAsync();
    }

    public async Task Remove(List<Collect> collects)
    {
        _context.Collect.RemoveRange(collects);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}