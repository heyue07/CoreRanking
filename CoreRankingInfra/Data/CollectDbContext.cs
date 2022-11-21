namespace CoreRankingInfra.Data;

public class CollectDbContext : DbContext
{
    public CollectDbContext(DbContextOptions<CollectDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collect>().Ignore(t => t.Amount);

        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CS_AS");

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Collect> Collect { get; set; }
}