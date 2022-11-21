namespace CoreRankingInfra.Data;

public class RoleDbContext : DbContext
{
    public RoleDbContext(DbContextOptions<RoleDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = ConnectionBuilder.GetConnectionString();

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        base.OnConfiguring(optionsBuilder);
    }*/

    public DbSet<Role> Role { get; set; }
}