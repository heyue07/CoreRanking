namespace CoreRankingDomain.Interfaces;

public interface IRoleRepository
{
    Task<Role> AddByID(int roleId);
    Task AddByAccountID(int accountId);
    Task RemoveByModel(Role role);
    Task RemoveByID(int roleId);
    Task<Role> GetRoleFromName(string characterName);
    Task<Role> GetRoleFromId(int roleId);
    Task<int> GetKill(int roleId);
    Task<int> GetKill(string characterName);
    Task<int> SaveChangesAsync();
    Task<List<Role>> GetTopRank(string classe);
    Task<List<Role>> GetTopLevelRank(string classe);
    Task<List<Role>> GetTopRankingByKDA(string classe);
    Task<List<Role>> GetTopRankingByCollectPoint(string classe);
    Task ResetKDA(int roleId);
    Task<double> GetKDA(string characterName);
    Task<KeyValuePair<int, int>> GetInteractions(string characterName);
    Task<Role> GetRoleFromIdNoTracking(int roleId);
    Task<List<Role>> GetAll();
    Task<bool> ExistAnyRecord();
    Task<bool> ExistAnyRecord(int roleId);
    Task<Role> GetRoleFromNameAsNoTracking(string characterName);
    Task IncrementRebornCount(int roleId);
    Task<double> GetCollectPoint(string characterName);
}