namespace CoreRanking.Watchers;

public class MultipleKillWatch : BackgroundService
{
    private static List<PlayerControl> ToRemove = new List<PlayerControl>();

    private static List<PlayerControl> PlayerController = new List<PlayerControl>();

    private static MultipleKill MultipleKill;

    private static IServerRepository _serverContext;

    private readonly ILogger<MultipleKillWatch> _logger;

    private static IRoleRepository _roleContext;

    private readonly LogWriter _logWriter;

    public MultipleKillWatch(ILogger<MultipleKillWatch> logger, LogWriter logWriter, MultipleKill multipleKill,
        IServerRepository serverContext, IRoleRepository roleContext)
    {
        MultipleKill = multipleKill;

        this._logWriter = logWriter;

        _serverContext = serverContext;

        _logger = logger;

        _roleContext = roleContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!MultipleKill.IsActive)
        {
            _logger.LogInformation("MÓDULO DE MULTIPLEKILL INICIADO");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (PlayerController.Count > 0)
                    {
                        foreach (var player in PlayerController)
                        {
                            if (player.Kills <= 2 && player.Clock.ElapsedMilliseconds - 100 > MultipleKill.DoubleKill.Time)
                            {
                                if (player.Kills == 2)
                                    await _serverContext.SendPrivateMessage(player.Role.RoleId, "SEQUÊNCIA DE MORTE FINALIZADA");

                                ToRemove.Add(player);
                            }
                            else if (player.Kills == 3 && player.Clock.ElapsedMilliseconds - 100 > MultipleKill.TripleKill.Time)
                            {
                                await _serverContext.SendPrivateMessage(player.Role.RoleId, "SEQUÊNCIA DE MORTE FINALIZADA");
                                ToRemove.Add(player);
                            }
                            else if (player.Kills == 4 && player.Clock.ElapsedMilliseconds - 100 > MultipleKill.QuadraKill.Time)
                            {
                                await _serverContext.SendPrivateMessage(player.Role.RoleId, "SEQUÊNCIA DE MORTE FINALIZADA");
                                ToRemove.Add(player);
                            }
                            else if (player.Kills >= 5 && player.Clock.ElapsedMilliseconds - 100 > MultipleKill.PentaKill.Time)
                            {
                                await _serverContext.SendPrivateMessage(player.Role.RoleId, "SEQUÊNCIA DE MORTE FINALIZADA");
                                ToRemove.Add(player);
                            }
                        }

                        PlayerController = PlayerController.Except(ToRemove).ToList();
                        ToRemove.Clear();
                    }

                    await Task.Delay(250);
                }
                catch (Exception ex)
                {
                    _logWriter.Write(ex.ToString());
                }
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
    public async static Task Trigger(Role Role)
    {
        try
        {
            if (MultipleKill.IsActive)
            {
                if (Role is null)
                    return;

                var player = PlayerController.Where(x => x.Role.CharacterName.Equals(Role.CharacterName)).FirstOrDefault();

                if (player is null)
                {
                    var newPlayer = new PlayerControl
                    {
                        Clock = new Stopwatch(),
                        Role = Role,
                        Kills = 1
                    };

                    PlayerController.Add(newPlayer);

                    newPlayer.Clock.Start();

                    return;
                }

                if (player.Clock.ElapsedMilliseconds <= MultipleKill.DoubleKill.Time && player.Kills.Equals(1))
                {
                    PlayerController.Where(x => x.Role.CharacterName.Equals(Role.CharacterName)).FirstOrDefault().Kills += 1;

                    await Reward(MultipleKill.DoubleKill, Role, player, 2);
                }
                else if (player.Clock.ElapsedMilliseconds <= MultipleKill.TripleKill.Time && player.Kills.Equals(2))
                {
                    PlayerController.Where(x => x.Role.CharacterName.Equals(Role.CharacterName)).FirstOrDefault().Kills += 1;

                    await Reward(MultipleKill.TripleKill, Role, player, 3);
                }
                else if (player.Clock.ElapsedMilliseconds <= MultipleKill.QuadraKill.Time && player.Kills.Equals(3))
                {
                    PlayerController.Where(x => x.Role.CharacterName.Equals(Role.CharacterName)).FirstOrDefault().Kills += 1;

                    await Reward(MultipleKill.QuadraKill, Role, player, 4);
                }
                else if (player.Clock.ElapsedMilliseconds <= MultipleKill.PentaKill.Time && player.Kills >= 4)
                {
                    PlayerController.Where(x => x.Role.CharacterName.Equals(Role.CharacterName)).FirstOrDefault().Kills += 1;

                    await Reward(MultipleKill.PentaKill, Role, player, 5);
                }
            }
        }
        catch (Exception ex)
        {
            LogWriter.StaticWrite(ex.ToString());
        }
    }
    private async static Task Reward(dynamic Multiplier, Role Role, PlayerControl Controller, int kills)
    {
        try
        {
            if (MultipleKill.IsMessageAllowed)
                _serverContext.SendMessage(MultipleKill.Channel, await BuildMessage(Multiplier.Messages, Role.CharacterName), Role.RoleId);

            var currentRole = await _roleContext.GetRoleFromId(Role.RoleId);

            if (currentRole != null)
            {
                currentRole.Points += Multiplier.Points;

                if (Multiplier is DoubleKill)
                {
                    currentRole.Doublekill += 1;
                    currentRole.Points += MultipleKill.DoubleKill.Points;

                    LogWriter.StaticWrite($"MultipleKill: {currentRole.CharacterName} fez Doublekill e ganhou {MultipleKill.DoubleKill.Points} pontos");
                }
                else if (Multiplier is TripleKill)
                {
                    currentRole.Triplekill += 1;
                    currentRole.Points += MultipleKill.TripleKill.Points;

                    LogWriter.StaticWrite($"MultipleKill: {currentRole.CharacterName} fez Triplekill e ganhou {MultipleKill.TripleKill.Points} pontos");
                }
                else if (Multiplier is QuadraKill)
                {
                    currentRole.Quadrakill += 1;
                    currentRole.Points += MultipleKill.QuadraKill.Points;

                    LogWriter.StaticWrite($"MultipleKill: {currentRole.CharacterName} fez Quadrakill e ganhou {MultipleKill.QuadraKill.Points} pontos");
                }
                else if (Multiplier is PentaKill)
                {
                    currentRole.Pentakill += 1;
                    currentRole.Points += MultipleKill.PentaKill.Points;

                    LogWriter.StaticWrite($"MultipleKill: {currentRole.CharacterName} fez Pentakill e ganhou {MultipleKill.PentaKill.Points} pontos");
                }
            }

            if (kills is 2)
                await _serverContext.SendPrivateMessage(Role.RoleId, "SEQUÊNCIA DE MORTE INICIADA");

            await _roleContext.SaveChangesAsync();

            Controller.Clock.Restart();

            PlayerController.Where(x => x.Role.CharacterName.Equals(Role.CharacterName)).FirstOrDefault().Clock = Controller.Clock;
        }
        catch (Exception ex)
        {
            LogWriter.StaticWrite(ex.ToString());
        }
    }

    private static async Task<string> BuildMessage(List<string> messages, string killer) => messages[new Random().Next(0, messages.Count - 1)].Replace("$killer", killer);
}