using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ZMK.Application.Services;
using ZMK.Domain.Entities;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation.Extensions;

public static class Registrator
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddTransient<IClock, Clock>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddIdentity();

        return services;
    }

    private static void AddIdentity(this IServiceCollection services)
    {
        services
        .AddIdentity<User, Role>(e =>
        {
            e.Password.RequireNonAlphanumeric = false;
            e.Password.RequireDigit = false;
            e.Password.RequireUppercase = false;
            e.Password.RequireLowercase = false;
        })
        .AddSignInManager<SignInManager<User>>()
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ZMKDbContext>()
        .AddDefaultTokenProviders();
    }
}