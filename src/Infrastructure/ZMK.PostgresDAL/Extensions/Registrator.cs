using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ZMK.PostgresDAL.Extensions;

public static class Registrator
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IDatabaseOptions databaseOptions)
    {
        services.AddDbContext<ZMKDbContext>(e =>
        {
            e.UseNpgsql(databaseOptions.ConnectionString);
        });
        return services;
    }
}
