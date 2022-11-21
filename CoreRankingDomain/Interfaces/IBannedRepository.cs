namespace CoreRankingDomain.Interfaces;

public interface IBannedRepository
{
    Task AddByModel(Banned banned);
    Task<List<Banned>> GetByRoleID(int roleId);
    Task RemoveByModel(Banned banned);
    Task SaveChangesAsync();
    Task<int> GetBanCount(int roleId);
    Task<bool> PlayerCurrentlyBanned(int roleId);
}
