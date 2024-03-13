using Microsoft.AspNetCore.Identity;
using ZMK.Domain.Constants;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Services;

public class DatabaseSeeder : IDatabaseSeeder
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

    private static readonly Employee FirstEmployee = new()
    {
        Remark = "Создан исключительно в целях тестирования, рекомендуется удалить.",
        FullName = "Тестовый Сотрудник",
        Post = "тест"
    };

    private static readonly User FirstUser = new()
    {
        Username = "TestAdmin",
        EmployeeId = FirstEmployee.Id,
    };

    private const string FIRST_PASSWORD = "string123";

    private readonly UserManager<User> _userManager;
    private readonly ZMKDbContext _dbContext;

    public DatabaseSeeder(
        UserManager<User> userManager, 
        ZMKDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task SeedDataAsync()
    {
        if (!IsAble())
        {
            return;
        }

        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            _dbContext.Roles.AddRange(Roles.Values);
            _dbContext.Employees.Add(FirstEmployee);
            await _dbContext.SaveChangesAsync();

            var result = await _userManager.CreateAsync(FirstUser, FIRST_PASSWORD);
            if (!result.Succeeded)
            {
                throw new Exception($"User was not added. {string.Join("\n", result.Errors.Select(e => $"{e.Code}.{e.Description}"))}");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(FirstUser, DefaultRoles.Admin);
            if (!addToRoleResult.Succeeded)
            {
                throw new Exception($"User was not added to role. {string.Join("\n", addToRoleResult.Errors.Select(e => $"{e.Code}.{e.Description}"))}");
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private bool IsAble()
    {
        return new bool[]
        {
            _dbContext.Employees.Any(),
            _dbContext.Users.Any(),
            _dbContext.Roles.Any(),
        }
        .All(e => e is false);
    }
}
