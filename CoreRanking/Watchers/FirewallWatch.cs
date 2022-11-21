namespace CoreRanking.Watchers;

public class FirewallWatch : BackgroundService
{
    private readonly FirewallDefinitions _definitions;

    private readonly ILogger<FirewallWatch> _logger;

    private readonly IServerRepository _serverContext;

    private readonly IBannedRepository _bannedContext;

    private readonly IRoleRepository _roleContext;

    private readonly IBattleRepository _battleContext;

    private readonly LogWriter _logWriter;

    private readonly ClassPointConfigFactory _classPointFactory;

    public FirewallWatch(ILogger<FirewallWatch> logger, LogWriter logWriter, IServerRepository server,
        IBannedRepository bannedRepository, IBattleRepository battleContext, IRoleRepository roleContext,
        FirewallDefinitions definitions, ClassPointConfigFactory classPointFactory)
    {
        this._logger = logger;
        this._logWriter = logWriter;
        this._definitions = definitions;
        this._serverContext = server;
        this._bannedContext = bannedRepository;
        this._battleContext = battleContext;
        this._roleContext = roleContext;
        this._classPointFactory = classPointFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_definitions.Active)
        {
            _logger.LogInformation("MÓDULO DE FIREWALL INICIADO");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Check();

                await Task.Delay(5000);
            }
        }
        else
        {
            await StopAsync(new CancellationToken(true));
        }
    }
    public override Task StartAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
    private async Task Check()
    {
        //Obtém todas as batalhas num tempo definido
         List<Battle> allBattles = await _battleContext.GetByDate(DateTime.Now.Subtract(TimeSpan.FromSeconds(_definitions.TimeLimit)));

        if (allBattles?.Count > 0)
        {
            var groupedBattles = allBattles.GroupBy(x => x.KillerId);

            var outlawPlayers = groupedBattles.Where(x => x.Count() > _definitions.KillLimit);

            foreach (var outlawPlayer in outlawPlayers)
            {
                if (await _bannedContext.PlayerCurrentlyBanned(outlawPlayer.Key)) continue;
                
                await Ban(outlawPlayer.Key);

                Role bannedRole = allBattles.Select(x => x.KillerRole).Where(x => x.RoleId.Equals(outlawPlayer.Key)).FirstOrDefault();

                await _serverContext.SendMessage(_definitions.Channel, $"RANKING FIREWALL: {bannedRole?.CharacterName} foi punido devido a FreeKill.", bannedRole.RoleId);

                await RemoveRecords(allBattles.Where(x => x.KillerId.Equals(outlawPlayer.Key) | x.KilledId.Equals(outlawPlayer.Key)).ToList());
            }
        }
    }

    private async Task Ban(int roleId)
    {
        try
        {
            var roleBans = await _bannedContext.GetBanCount(roleId);

            await _bannedContext.AddByModel(new Banned
            {
                RoleId = roleId,
                BanTime = DateTime.Now.AddSeconds(_definitions.BanTime * ++roleBans)
            });

            await _bannedContext.SaveChangesAsync();

            _logWriter.Write($"Role {roleId} foi banido por {_definitions.BanTime * roleBans} segundos devido ao Firewall de PvP.");
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task RemoveRecords(List<Battle> BattleToRemove)
    {
        try
        {
            if (BattleToRemove?.Count > 0)
            {                
                foreach (var battle in BattleToRemove)
                {
                    battle.KilledRole.Death -= 1;
                    battle.KillerRole.Kill -= 1;

                    battle.KillerRole.Points -= _classPointFactory
                        .Get()
                        .Where(x => x.Ocuppation.Equals(battle.KillerRole.CharacterClass.ConvertClassFromGameStructure()))
                        .Select(x => x.onKill)
                        .FirstOrDefault();
                }

                await _battleContext.RemoveRange(BattleToRemove);

                await _battleContext.SaveChangesAsync();

                await _roleContext.SaveChangesAsync();

                _logWriter.Write($"Ranking Firewall: foram removidos {BattleToRemove.Count} registros de PvP.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
}