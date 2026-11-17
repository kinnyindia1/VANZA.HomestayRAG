using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace VANZA.HomestayAI.API.Data;

public class VanzaDbContextFactory
    : IDesignTimeDbContextFactory<VanzaDbContext>
{
    public VanzaDbContext CreateDbContext(string[] args)
    {
        // Read the connection string from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception(
                "DefaultConnection not found in appsettings.json");
        }

        // Create database connection options
        var optionsBuilder =
            new DbContextOptionsBuilder<VanzaDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        // Create and return the database context
        return new VanzaDbContext(optionsBuilder.Options);
    }
}
