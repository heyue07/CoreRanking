namespace CoreRanking.Watchers;

public class PrizeWatch : BackgroundService
{
    private readonly IPrizeRepository prizeContext;
    private readonly PrizeDefinitions settings;
    private readonly LogWriter logger;
    private readonly IServerRepository serverContext;
    private readonly TimeSpan delayTime;

    public PrizeWatch(LogWriter logger, IPrizeRepository prizeContext, PrizeDefinitions settings, IServerRepository serverContext)
    {
        this.prizeContext = prizeContext;
        this.settings = settings;
        this.logger = logger;
        this.serverContext = serverContext;
        this.delayTime = GetTimeBySettings();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (settings.Active)
        {
            logger.Write("MÓDULO DE RECOMPENSAS INICIADO");

            if (await prizeContext.IsFirstRun())
                await RunProcedure();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var lastRewardDate = await prizeContext.GetLastRewardDate();

                    if (DateTime.Now >= lastRewardDate.Add(delayTime) & settings.DeliveryPrizeHour.Equals(DateTime.Now.Hour))
                    {
                        await RunProcedure();
                    }
                    else
                    {
                        logger.Write($"MÓDULO DE RECOMPENSA: {DateTime.Now.DayOfWeek} - Fora dos prazos. Entrando em descanso por 1 hora.");

                        await Task.Delay(TimeSpan.FromHours(1));
                    }
                }
                catch (Exception ex)
                {
                    logger.Write(ex.ToString());
                }
            }
        }
        else
        {
            await StopAsync(new CancellationToken(true));
        }
    }

    private async Task RunProcedure()
    {
        try
        {
            var response = await prizeContext.DeliverTopRankingReward();

            if (response is null) return;

            var prizeRecord = GeneratePrizeEntity(response);

            if (prizeRecord != null)
                await prizeContext.Add(prizeRecord);

            if (settings.ResetRankingAfterPrizeDelivery)
            {
                await prizeContext.ResetRanking();
                await serverContext.SendMessage(BroadcastChannel.System, "O ranking foi reiniciado! Aproveite para ser um dos primeiros dessa vez!");

                logger.Write("O ranking foi resetado.");
            }

            await prizeContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.Write(ex.ToString());
        }
    }

    private Prize GeneratePrizeEntity(DeliverRewardResponse response)
    {
        var isCashReward = settings.PrizeRewardType.Equals(EPrizeReward.Cash);

        Prize prizeRecord = new Prize
        {
            CashCount = isCashReward ? response.Reward : 0,
            DeliveryRoleIdListAsJson = JsonConvert.SerializeObject(response.PlayersIdRewarded),
            WinCriteria = settings.WinCriteria,
            DeliveryCount = response.PlayersIdRewarded.Count,
            ItemRewardCount = isCashReward ? 0 : response.Reward.Count,
            ItemRewardId = isCashReward ? 0 : response.Reward.Id,
            PrizeDeliveryDate = DateTime.Now,
            PrizeOption = settings.PrizeFrequencyOption,
            PrizeRewardType = settings.PrizeRewardType,
        };

        return prizeRecord;
    }
    private TimeSpan GetTimeBySettings()
    {
        return settings.PrizeFrequencyOption switch
        {
            EPrizeFrequencyOption.Diario => TimeSpan.FromDays(1),
            EPrizeFrequencyOption.Semanal => TimeSpan.FromDays(7),
            EPrizeFrequencyOption.Quinzenal => TimeSpan.FromDays(15),
            EPrizeFrequencyOption.Mensal => TimeSpan.FromDays(30),
            EPrizeFrequencyOption.Trimestral => TimeSpan.FromDays(90),
            EPrizeFrequencyOption.Semestral => TimeSpan.FromDays(180),
            _ => TimeSpan.FromDays(7)
        };
    }

    public override Task StartAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}