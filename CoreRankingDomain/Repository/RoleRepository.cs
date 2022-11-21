namespace CoreRankingDomain.Repository;

public class RoleRepository : IRoleRepository
{
    private readonly RoleDbContext _context;
    private readonly RankingDefinitions _definitions;
    private readonly IServerRepository _serverContext;
    private readonly IAccountRepository _accountContext;
    private readonly ICollectRepository _collectContext;
    private readonly IHuntRepository _huntContext;
    private readonly LogWriter _logWriter;
    private List<int> gmRoleIDs;

    public RoleRepository(RoleDbContext context, LogWriter logWriter, RankingDefinitions rankingDefinitions, IServerRepository serverContext,
        IAccountRepository accountRepository, ICollectRepository collectContext, IHuntRepository huntContext)
    {
        this._context = context;
        this._accountContext = accountRepository;
        this._definitions = rankingDefinitions;
        this._serverContext = serverContext;
        this._logWriter = logWriter;
        this._collectContext = collectContext;
        this._huntContext = huntContext;
    }

    public async Task<Role> AddByID(int role)
    {
        try
        {
            if (await _context.Role.AsNoTracking().AnyAsync(x => x.RoleId.Equals(role)))
                return default;

            GRoleData roleBase = await _serverContext.GetRoleByID(role);

            if (roleBase != null)
            {
                var roleData = roleBase.ToRole();

                var account = await _accountContext.GetByID(roleData.AccountId);

                if (account is null)
                {
                    await _accountContext.Add(new Account { Id = roleData.AccountId });

                    await _accountContext.SaveChangesAsync();

                    _logWriter.Write($"Conta de ID {roleBase.GRoleBase.UserId} foi incluída no Ranking via Role -> AddByID.");
                }

                _context.Role.Add(roleData);

                await _context.SaveChangesAsync();

                _logWriter.Write($"O personagem {roleData.CharacterName} foi incluído no Ranking via ID.");

                return roleData;
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<double> GetCollectPoint(string characterName)
    {
        try
        {
            return await _context.Role
                .Where(x => x.CharacterName.Equals(characterName))
                .Select(x => x.CollectPoint)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task IncrementRebornCount(int roleId)
    {
        try
        {
            var role = await GetRoleFromId(roleId);

            role.RebornCount++;
            role.LevelDate = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task<int> GetKill(int roleId)
    {
        try
        {
            return await _context.Role
                .Where(x => x.RoleId.Equals(roleId))
                .Select(x => x.Kill)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<int> GetKill(string characterName)
    {
        try
        {
            return await _context.Role
                .Where(x => x.CharacterName.Equals(characterName))
                .Select(x => x.Kill)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<Role> GetRoleFromId(int roleId)
    {
        try
        {
            Role role = await _context.Role.FindAsync(roleId);

            return role;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<Role> GetRoleFromIdNoTracking(int roleId)
    {
        try
        {
            Role role = await _context.Role
                .AsNoTracking()
                .Where(x => x.RoleId.Equals(roleId))
                .FirstOrDefaultAsync();

            return role;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<Role> GetRoleFromName(string characterName)
    {
        try
        {
            return await _context.Role
                .Where(x => x.CharacterName.Equals(characterName))
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<Role> GetRoleFromNameAsNoTracking(string characterName)
    {
        try
        {
            return await _context.Role
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CharacterName.Equals(characterName));
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<List<Role>> GetTopRank(string convertedClass)
    {
        try
        {
            this.gmRoleIDs = gmRoleIDs ?? await _serverContext.GetGmID();

            List<Role> topPlayers = new();

            if (!string.IsNullOrEmpty(convertedClass))
            {
                topPlayers = await _context.Role
                    .AsNoTracking()
                    .Where(x => !gmRoleIDs.Contains(x.RoleId) & x.CharacterClass.ToUpper().Equals(convertedClass.ToUpper()))                    
                    .OrderByDescending(x => x.Kill)
                    .Take(_definitions.AmountPlayersOnPodium)
                    .ToListAsync();
            }
            else
            {
                topPlayers = await _context.Role
                    .AsNoTracking()
                    .Where(x => !gmRoleIDs.Contains(x.RoleId))
                    .OrderByDescending(x => x.Kill)
                    .Take(_definitions.AmountPlayersOnPodium)
                    .ToListAsync();
            }

            return topPlayers?.Where(x => x != null).ToList();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<List<Role>> GetTopRankingByKDA(string convertedClass)
    {
        try
        {
            this.gmRoleIDs = gmRoleIDs ?? await _serverContext.GetGmID();

            List<Role> topPlayers = new();

            if (string.IsNullOrEmpty(convertedClass))
            {
                topPlayers = await _context.Role
                .AsNoTracking()
                .OrderByDescending(x => x.Kill / x.Death)
                .Where(x => !gmRoleIDs.Contains(x.RoleId) & x.Kill > 0)
                .Take(_definitions.AmountPlayersOnPodium)
                .ToListAsync();
            }
            else
            {
                topPlayers = await _context.Role
                .AsNoTracking()
                .OrderByDescending(x => x.Kill / x.Death)
                .Where(x => !gmRoleIDs.Contains(x.RoleId) & x.CharacterClass.ToUpper().Equals(convertedClass.ToUpper()) & x.Kill > 0)
                .Take(_definitions.AmountPlayersOnPodium)
                .ToListAsync();
            }

            return topPlayers;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<List<Role>> GetTopLevelRank(string convertedClass)
    {
        try
        {
            this.gmRoleIDs = gmRoleIDs ?? await _serverContext.GetGmID();

            gmRoleIDs.Clear();

            List<Role> topPlayers = new();

            if (!string.IsNullOrEmpty(convertedClass))
            {
                topPlayers = await _context.Role
                    .AsNoTracking()
                    .Where(x => !gmRoleIDs.Contains(x.RoleId) & x.CharacterClass.Equals(convertedClass.ConvertClassToGameStructure()))
                    .OrderByDescending(y => y.RebornCount)
                    .ThenByDescending(y => y.Level)
                    .ThenBy(y => y.LevelDate)
                    .Take(_definitions.AmountPlayersOnPodium)
                    .ToListAsync();
            }
            else
            {
                topPlayers = await _context.Role
                    .AsNoTracking()
                    .Where(x => !gmRoleIDs.Contains(x.RoleId))
                    .OrderByDescending(y => y.RebornCount)
                    .ThenByDescending(x => x.Level)
                    .ThenBy(y => y.LevelDate)
                    .Take(_definitions.AmountPlayersOnPodium)
                    .ToListAsync();
            }

            return topPlayers;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<List<Role>> GetTopRankingByCollectPoint(string convertedClass)
    {
        try
        {
            this.gmRoleIDs = gmRoleIDs ?? await _serverContext.GetGmID();

            List<Role> topPlayers = new();

            if (!string.IsNullOrEmpty(convertedClass))
            {
                topPlayers = await _context.Role
                    .AsNoTracking()
                    .Where(x => !gmRoleIDs.Contains(x.RoleId) & x.CollectPoint > 0 & x.CharacterClass.Equals(convertedClass.ConvertClassToGameStructure()))
                    .OrderByDescending(y => y.CollectPoint)
                    .Take(_definitions.AmountPlayersOnPodium)
                    .ToListAsync();
            }
            else
            {
                topPlayers = await _context.Role
                    .AsNoTracking()
                    .Where(x => x.CollectPoint > 0 & !gmRoleIDs.Contains(x.RoleId))
                    .OrderByDescending(y => y.CollectPoint)
                    .Take(_definitions.AmountPlayersOnPodium)
                    .ToListAsync();
            }

            return topPlayers;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task RemovePveRecords(int roleId)
    {
        var hunts = await _huntContext.GetHuntsByRole(roleId);

        if (hunts != null)
        {
            await _huntContext.Remove(hunts);

            await _huntContext.SaveChangesAsync();
        }

        var collects = await _collectContext.GetCollectsByRole(roleId);

        if (collects != null)
        {
            await _collectContext.Remove(collects);

            await _collectContext.SaveChangesAsync();
        }
    }

    public async Task RemoveByModel(Role role)
    {
        try
        {
            if (role != null)
            {
                await RemovePveRecords(role.RoleId);

                _context.Role.Remove(role);

                _logWriter.Write($"O personagem {role.CharacterName} foi excluído do Ranking via DbModel.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task RemoveByID(int roleId)
    {
        try
        {
            Role roleToDelete = await _context.Role.FindAsync(roleId);

            if (roleToDelete != null)
            {
                await RemovePveRecords(roleToDelete.RoleId);

                _context.Role.Remove(roleToDelete);

                _logWriter.Write($"O personagem {roleToDelete.CharacterName} foi excluído do Ranking via ID.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public async Task AddByAccountID(int accountId)
    {
        try
        {
            List<GRoleData> accountRoles = await _serverContext.GetRolesFromAccount(accountId);

            if (accountRoles is null) return;

            List<Role> roles = accountRoles?.Select(x => x.ToRole()).ToList();

            if (roles is null) return;

            var filteredRoles = roles.Where(r => !_context.Role.Select(x => x.RoleId).Contains(r.RoleId)).ToList();

            if (filteredRoles != null)
                _context.Role.AddRange(roles);

            foreach (var role in filteredRoles)
            {
                await _serverContext.SendPrivateMessage(role.RoleId, "Você foi inserido no CoreRanking. Digite !help para instruções de comandos.");

                _logWriter.Write($"Personagem ID {role.RoleId} adicionado ao ranking.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task ResetKDA(int roleId)
    {
        Role role = await _context.Role.FindAsync(roleId);

        if (role != null)
        {
            role.Kill = 0;
            role.Death = 0;
        }
    }

    public async Task<double> GetKDA(string roleName)
    {
        try
        {
            double kda = 0;

            Role role = await GetRoleFromNameAsNoTracking(roleName);

            if (role != null)
            {
                double kills = role.Kill;
                double deaths = role.Death;

                if (kills is 0)
                    kda = 0;

                if (deaths is 0 & kills > 0)
                    kda = kills;

                if (kills > 0 & deaths > 0)
                    kda = kills / deaths;
            }

            return kda;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<KeyValuePair<int, int>> GetInteractions(string characterName)
    {
        try
        {
            int kills = 0, deaths = 0;

            Role role = await GetRoleFromNameAsNoTracking(characterName);

            if (role != null)
            {
                kills = role.Kill;
                deaths = role.Death;
            }

            return new KeyValuePair<int, int>(kills + deaths, kills);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<List<Role>> GetAll() => await _context.Role.ToListAsync();

    public async Task<bool> ExistAnyRecord()
    {
        return await _context.Role
            .AsNoTracking()
            .AnyAsync();
    }
    public async Task<bool> ExistAnyRecord(int roleId)
    {
        return await _context.Role
            .AsNoTracking()
            .AnyAsync(x => x.RoleId
            .Equals(roleId));
    }    
}