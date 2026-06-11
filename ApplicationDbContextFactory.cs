using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ThisisczApi;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Design-time defaults to postgres (appsettings.json). Load Local.json for
        // real connection strings; do not load Development.json (sqlite corrupts migrations).
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var databaseProvider =
            configuration.GetValue<string>("DatabaseProvider")?.Trim().ToLowerInvariant()
            ?? "postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (databaseProvider == "sqlite")
        {
            optionsBuilder.UseSqlite(configuration.GetConnectionString("SQLITE_CONNECTIONSTRING"));
        }
        else
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("POSTGRES_CONNECTIONSTRING"));
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
