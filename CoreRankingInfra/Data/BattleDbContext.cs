namespace CoreRankingInfra.Data;

public class BattleDbContext : DbContext
{
    public BattleDbContext(DbContextOptions<BattleDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    public DbSet<Battle> Battle { get; set; }
}