namespace CoreRankingDomain.Repository;

public class PrizeRepository : IPrizeRepository
{
    private readonly RoleDbContext _roleContext;
    private readonly IServerRepository _serverContext;
    private readonly LogWriter logger;
    private readonly PrizeDefinitions definitions;
    private readonly PrizeDbContext _context;
    private readonly BattleDbContext _battleContext;

    public PrizeRepository(PrizeDbContext prizeContext, PrizeDefinitions definitions, LogWriter logger,
        RoleDbContext context, IServerRepository _serverContext, BattleDbContext battleContext)
    {
        this._roleContext = context;
        this._serverContext = _serverContext;
        this.logger = logger;
        this.definitions = definitions;
        this._context = prizeContext;
        this._battleContext = battleContext;
    }

    public async Task ResetRanking()
    {
        try
        {
            var roles = await _roleContext.Role.ToListAsync();

            _battleContext.Battle.RemoveRange(await _battleContext.Battle.ToListAsync());

            foreach (var role in roles)
            {
                role.Kill = 0;
                role.Death = 0;
            }
        }
        catch (Exception ex)
        {
            logger.Write(ex.ToString());
        }
    }
    public async Task<DeliverRewardResponse> DeliverTopRankingReward()
    {
        var reward = await GetReward();

        var roles = definitions.WinCriteria switch
        {
            EWinCriteria.Kill => await GetRolesByKillToDeliveryReward(),
            EWinCriteria.KDA => await GetRolesByKDAToDeliveryReward(),
            EWinCriteria.PVE => await GetRolesByCollectPointToDeliveryReward(),
            _ => null
        };

        if (roles is null) return null;

        if (definitions.PrizeRewardType.Equals(EPrizeReward.Cash))
        {
            foreach (var role in roles)
            {
                await _serverContext.GiveCash(role.AccountId, (int)reward);
                await _serverContext.SendPrivateMessage(role.RoleId, "Sua recompensa de top ranking chegou! Relogue e cheque seu gshop");

                logger.Write($"Recompensa de top ranking entregue ao personagem {role.CharacterName}");
            }
        }
        else
        {
            foreach (var role in roles)
            {
                await _serverContext.DeliverReward(definitions.ItemAward,
                    definitions.ItemAward.Count, role.RoleId,
                    "Recompensa de Top Ranking",
                    "Você ficou em uma boa colocação no ranking e recebeu uma recompensa por isso. Continue se esforçando!");

                await _serverContext.SendPrivateMessage(role.RoleId, "Sua recompensa de top ranking chegou! Dê uma olhada na caixa de correio");

                logger.Write($"Recompensa de top ranking entregue ao personagem {role.CharacterName}");
            }
        }

        return new DeliverRewardResponse(roles.Select(x => x.RoleId).ToList(), reward);
    }
    public async Task Add(Prize prize)
    {
        _context.Prize.Add(prize);
    }
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public async Task<bool> IsFirstRun() => !await _context.Prize.AnyAsync();
    private async Task<List<Role>> GetRolesByKillToDeliveryReward()
    {
        return await _roleContext.Role
            .AsNoTracking()
            .OrderByDescending(x => x.Kill)
            .Take(definitions.DeliverRewardToFirstAmountPlayers)
            .ToListAsync();
    }
    private async Task<List<Role>> GetRolesByKDAToDeliveryReward()
    {
        return await _roleContext.Role
            .AsNoTracking()
            .OrderByDescending(x => x.Kill / x.Death)
            .Take(definitions.DeliverRewardToFirstAmountPlayers)
            .ToListAsync();
    }
    private async Task<List<Role>> GetRolesByCollectPointToDeliveryReward()
    {
        return await _roleContext.Role
            .AsNoTracking()
            .OrderByDescending(x => x.CollectPoint)
            .Take(definitions.DeliverRewardToFirstAmountPlayers)
            .ToListAsync();
    }

    private async Task<dynamic> GetReward()
    {
        return definitions.PrizeRewardType.Equals(EPrizeReward.Cash) ? (int)definitions.CashCount : (ItemAward)definitions.ItemAward;
    }

    public async Task<DateTime> GetLastRewardDate()
    {
        return await _context.Prize
            .AsNoTracking()
            .OrderBy(x => x.PrizeDeliveryDate)
            .Select(x => x.PrizeDeliveryDate)
            .LastOrDefaultAsync();
    }
}