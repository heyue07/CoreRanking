CheckProcess();

string connectionString = ConnectionBuilder.GetConnectionString();

await Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddTransient<Program>();

        services.AddDbContext<AccountDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddDbContext<RoleDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddDbContext<BattleDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddDbContext<HuntDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddDbContext<BannedDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddDbContext<CollectDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        services.AddDbContext<PrizeDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        //services.AddHostedService<LicenseControl>();
        services.AddHostedService<PvPWatch>();
        services.AddHostedService<PvEWatch>();
        services.AddHostedService<RoleWatch>();
        services.AddHostedService<FirewallWatch>();
        services.AddHostedService<TriggersWatch>();
        services.AddHostedService<MultipleKillWatch>();
        services.AddHostedService<PrizeWatch>();

        services.AddSingleton<ClassPointConfig>();
        services.AddSingleton<FirewallDefinitions>();
        services.AddSingleton<ItemAward>();
        services.AddSingleton<MultipleKill>();
        services.AddSingleton<PveConfiguration>();
        services.AddSingleton<RankingDefinitions>();
        services.AddSingleton<ServerConnection>();
        services.AddSingleton<CoreLicense>();
        services.AddSingleton<PrizeDefinitions>();

        services.AddTransient<IBattleRepository, BattleRepository>();
        services.AddTransient<IRoleRepository, RoleRepository>();
        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<IHuntRepository, HuntRepository>();
        services.AddTransient<ICollectRepository, CollectRepository>();
        services.AddTransient<IBannedRepository, BannedRepository>();
        services.AddTransient<IServerRepository, ServerRepository>();
        services.AddTransient<IPrizeRepository, PrizeRepository>();

        services.AddSingleton<MessageFactory>();
        services.AddSingleton<ClassPointConfigFactory>();
        services.AddSingleton<ItemAwardFactory>();

        services.AddTransient<LogWriter>();

        services.AddLogging(builder =>
        {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("NToastNotify", LogLevel.Warning)
                   .AddConsole();
        });
    }).Build().RunAsync();

static void CheckProcess()
{
    Console.WriteLine("CHECANDO PROCESSOS EXISTENTES\n");

    Process p = Process.GetCurrentProcess();
    var ProcessesList = Process.GetProcessesByName(p.ProcessName);

    for (int i = 0; i < ProcessesList.Length - 1; i++)
    {
        if (!ProcessesList[i].Equals(p))
        {
            ProcessesList[i].Kill();
            Console.WriteLine("ELIMINANDO PROCESSO PRÉ-EXISTENTE");
        }
    }
}