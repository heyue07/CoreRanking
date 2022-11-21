namespace CoreRankingDomain.Repository;

public class AccountRepository : IAccountRepository
{
    private readonly AccountDbContext _context;
    private readonly IServerRepository _serverContext;
    private readonly LogWriter _logWriter;

    public AccountRepository(AccountDbContext context, LogWriter logWriter, IServerRepository serverContext)
    {
        this._context = context;
        this._serverContext = serverContext;
        this._logWriter = logWriter;
    }

    public async Task<Account> Add(Account Account)
    {
        try
        {
            if (!await _context.Account.AsNoTracking().AnyAsync(x => x.Id.Equals(Account.Id)))
            {
                var account = _context.Account.Add(Account);

                return account.Entity;
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task Remove(Account Account)
    {
        try
        {
            var accountToRemove = await _context.Account.FindAsync(Account.Id);

            if (accountToRemove != null)
                _context.Account.Remove(accountToRemove);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    public async Task Update(Account Account)
    {
        try
        {
            Account curAccount = await _context.Account.FirstOrDefaultAsync(x => x.Id.Equals(Account.Id));

            if (curAccount is null)
            {
                _context.Account.Add(Account);
            }
            else
            {
                _context.Entry(curAccount).CurrentValues.SetValues(Account);
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task<Account> GetByID(int accountId)
    {
        try
        {
            Account account = await _context.Account.FirstOrDefaultAsync(x => x.Id.Equals(accountId));

            return account;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task AddByRoleID(int roleId)
    {
        try
        {
            GRoleData roleData = await _serverContext.GetRoleByID(roleId);

            if (roleData != null)
                await Add(new Account { Id = roleData.GRoleBase.UserId });
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task<Account> GetByAccountID(int accountId)
    {
        try
        {
            return await _context.Account.Where(x => x.Id.Equals(accountId)).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<string> GetIpByAccountID(int accountId)
    {
        try
        {
            return await _context.Account.AsNoTracking().Where(x => x.Id.Equals(accountId)).Select(x => x.Ip).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}