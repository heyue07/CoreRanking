namespace CoreRankingInfra.Data;

public class PrizeDbContext : DbContext
{
    public PrizeDbContext(DbContextOptions<PrizeDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    string connectionString = ConnectionBuilder.GetConnectionString();

    //    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    //    base.OnConfiguring(optionsBuilder);
    //}

    public DbSet<Prize> Prize { get; set; }
}