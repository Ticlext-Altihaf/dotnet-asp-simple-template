using Microsoft.EntityFrameworkCore;

namespace BozoAIAggregator.Data;

public class DatabaseContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<Models.Status> Statuses { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=BozoAIAggregator.db");
        base.OnConfiguring(optionsBuilder);
    }
}
