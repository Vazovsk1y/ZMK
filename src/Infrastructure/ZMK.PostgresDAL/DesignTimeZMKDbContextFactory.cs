using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZMK.PostgresDAL;

public class DesignTimeZMKDbContextFactory : IDesignTimeDbContextFactory<ZMKDbContext>
{
    private const string ConnectionString = "User ID=postgres;Password=12345678;Host=localhost;Port=5432;Database=ZMKdb;";
    public ZMKDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseNpgsql(ConnectionString);
        return new ZMKDbContext(optionsBuilder.Options);
    }
}