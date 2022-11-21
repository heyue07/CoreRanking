namespace CoreRankingInfra.Data;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = ConnectionBuilder.GetConnectionString();

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        base.OnConfiguring(optionsBuilder);
    }*/

    public DbSet<Account> Account { get; set; }
}
