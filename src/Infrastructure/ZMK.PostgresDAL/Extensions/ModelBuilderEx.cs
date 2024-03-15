using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Constants;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Extensions;

public static class ModelBuilderEx
{
    private static readonly Dictionary<string, Role> Roles = new()
    {
        { DefaultRoles.Admin, new Role() {
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            Name = DefaultRoles.Admin,
            Description = "Администратор системы имеет право добавлять/изменять любые настройки и проэкты. Определяет текущую базу и ее местоположение.",
            NormalizedName = DefaultRoles.Admin.ToUpperInvariant(),
        }},
        { DefaultRoles.User, new Role() {
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            Name = DefaultRoles.User,
            Description = "Пользователь имеет право вносить выполнение по маркам, создавать и изменять отгрузки.",
            NormalizedName = DefaultRoles.User.ToUpperInvariant(),
        }},
        { DefaultRoles.Reader, new Role() {
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            Name = DefaultRoles.Reader,
            Description = "Доступ к проэктам с правом просмотра данных.",
            NormalizedName = DefaultRoles.Reader.ToUpperInvariant(),
        }},
    };

    private static readonly Employee TestEmployee = new()
    {
        Remark = "Создан исключительно в целях тестирования, рекомендуется удалить.",
        FullName = "Тестовый Сотрудник",
        Post = "Тестовый Сотрудник"
    };

    private static readonly User TestUser = new()
    {
        UserName = "TestAdmin",
        NormalizedUserName = "TestAdmin".ToUpperInvariant(),
        EmployeeId = TestEmployee.Id,
        LockoutEnabled = true,
    };

    private static readonly UserRole TestUserRole = new()
    {
        RoleId = Roles[DefaultRoles.Admin].Id,
        UserId = TestUser.Id,
    };

    private static readonly Area[] Areas =
    [
        new()
        {
            Order = 1,
            Title = "КМД",
        },
        new()
        {
            Title = "ЛСБ",
            Order = 2,
        },
        new()
        {
            Title = "Сборка",
            Order = 3,
        },
        new()
        {
            Title = "Сварка",
            Order = 4
        },
        new()
        {
            Title = "Зачистка",
            Order = 5,
        }
    ];

    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(Roles.Values);
        modelBuilder.Entity<Employee>().HasData(TestEmployee);
        modelBuilder.Entity<User>().HasData(TestUser);
        modelBuilder.Entity<UserRole>().HasData(TestUserRole);
        modelBuilder.Entity<Area>().HasData(Areas);
    }
}
