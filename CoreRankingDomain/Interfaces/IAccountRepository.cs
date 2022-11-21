namespace CoreRankingDomain.Interfaces;

public interface IAccountRepository
{
    Task<Account> Add(Account Account);
    Task<Account> GetByAccountID(int accountId);
    Task<string> GetIpByAccountID(int accountId);
    Task AddByRoleID(int roleId);
    Task Remove(Account Account);
    Task Update(Account Account);
    Task<Account> GetByID(int accountId);
    Task SaveChangesAsync();
}
