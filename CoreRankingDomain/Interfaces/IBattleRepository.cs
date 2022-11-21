namespace CoreRankingDomain.Interfaces;

public interface IBattleRepository
{
    Task<List<Battle>> GetByDate(DateTime fromThere);
    Task AddByModel(Battle battle);
    Task Remove(Battle battle);
    Task RemoveRange(List<Battle> battles);
    Task SaveChangesAsync();
    Task<List<Battle>> GetPlayerBattles(int roleId);
}
