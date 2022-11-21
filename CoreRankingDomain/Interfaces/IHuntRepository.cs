namespace CoreRankingDomain.Interfaces;

public interface IHuntRepository
{
    Task AddByModel(Hunt hunt);
    Task AddByModel(List<Hunt> hunts);
    Task Remove(Hunt hunt);
    Task Remove(List<Hunt> hunts);
    Task SaveChangesAsync();
    Task<List<Hunt>> GetHuntsByRole(int roleId);
}
