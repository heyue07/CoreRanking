namespace CoreRankingDomain.Repository;

public class BattleRepository : IBattleRepository
{
    private readonly BattleDbContext _context;

    public BattleRepository(BattleDbContext context)
    {
        _context = context;
    }

    public async Task AddByModel(Battle battle)
    {
        _context.Battle.Add(battle.Prepare());
    }

    public async Task Remove(Battle battle)
    {
        _context.Battle.Remove(battle);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRange(List<Battle> battles)
    {
        var toRemove = _context.Battle.Where(x => battles.Select(x => x.Id).Contains(x.Id));

        _context.Battle.RemoveRange(toRemove);
    }

    public async Task<List<Battle>> GetByDate(DateTime fromThere)
    {
        return await _context.Battle
            .Include(x => x.KillerRole)
            .Include(x => x.KilledRole)
            .Where(x => x.Date >= fromThere)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Battle>> GetPlayerBattles(int roleId)
    {        
        return await _context.Battle.Where(x => x.KilledId.Equals(roleId) | x.KillerId.Equals(roleId)).ToListAsync();
    }
}