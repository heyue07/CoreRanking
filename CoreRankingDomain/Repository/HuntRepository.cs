namespace CoreRankingDomain.Repository;

public class HuntRepository : IHuntRepository
{
    private readonly HuntDbContext _context;

    public HuntRepository(HuntDbContext context)
    {
        _context = context;
    }

    public async Task AddByModel(Hunt hunt)
    {
        _context.Hunt.Add(hunt);
    }
    public async Task AddByModel(List<Hunt> hunts)
    {
        _context.Hunt.AddRange(hunts);
    }

    public async Task<List<Hunt>> GetHuntsByRole(int roleId)
    {
        return await _context.Hunt.Where(x => x.RoleId.Equals(roleId)).ToListAsync();
    }

    public async Task Remove(Hunt hunt)
    {
        _context.Hunt.Remove(hunt);
    }

    public async Task Remove(List<Hunt> hunts)
    {
        _context.Hunt.RemoveRange(hunts);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}