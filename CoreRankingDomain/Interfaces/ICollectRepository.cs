namespace CoreRankingDomain.Interfaces;

public interface ICollectRepository
{
    Task AddByModel(Collect collect);
    Task<List<Collect>> GetCollectsByRole(int roleId);
    Task Remove(List<Collect> collects);
    Task SaveChangesAsync();
}
