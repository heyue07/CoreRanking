namespace CoreRankingInfra.Data;

public class HuntDbContext : DbContext
{
    public HuntDbContext(DbContextOptions<HuntDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    public DbSet<Hunt> Hunt { get; set; }
}
