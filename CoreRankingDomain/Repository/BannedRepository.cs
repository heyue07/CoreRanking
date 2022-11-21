namespace CoreRankingDomain.Repository;

public class BannedRepository : IBannedRepository
{
    private readonly BannedDbContext _context;

    public BannedRepository(BannedDbContext context)
    {
        _context = context;
    }

    public async Task AddByModel(Banned banned)
    {
        _context.Banned.Add(banned);
    }

    public async Task<int> GetBanCount(int roleId)
    {
        return await _context.Banned.CountAsync(x => x.RoleId.Equals(roleId));
    }

    public async Task<List<Banned>> GetByRoleID(int roleId)
    {
        return await _context.Banned.Where(x => x.RoleId.Equals(roleId)).ToListAsync();
    }

    public async Task<bool> PlayerCurrentlyBanned(int roleId)
    {
        return await _context.Banned.AnyAsync(x => x.RoleId.Equals(roleId) & x.BanTime > DateTime.Now);
    }

    public async Task RemoveByModel(Banned banned)
    {
        _context.Banned.Remove(banned);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}