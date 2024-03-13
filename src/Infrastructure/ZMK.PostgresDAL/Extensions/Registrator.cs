using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZMK.PostgresDAL.Services;

namespace ZMK.PostgresDAL.Extensions;

public static class Registrator
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IDatabaseOptions databaseOptions)
    {
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddDbContext<ZMKDbContext>(e =>
        {
            e.UseNpgsql(databaseOptions.ConnectionString);
        });
        return services;
    }
}
