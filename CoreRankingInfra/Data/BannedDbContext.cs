namespace CoreRankingInfra.Data;

public class BannedDbContext : DbContext
{
    public BannedDbContext(DbContextOptions<BannedDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    public DbSet<Banned> Banned { get; set; }
}