using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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

    private static readonly Employee TestEmployee = new()
    {
        Remark = "Создан исключительно в целях тестирования, рекомендуется удалить.",
        FullName = "Тестовый Сотрудник",
        Post = "тест"
    };

    private static readonly User TestUser = new()
    {
        Username = "TestAdmin",
        EmployeeId = TestEmployee.Id,
    };

    private const string TEST_PASSWORD = "string123";

    private readonly UserManager<User> _userManager;
    private readonly ZMKDbContext _dbContext;
    private readonly ILogger _logger;

    public DatabaseSeeder(
        UserManager<User> userManager,
        ZMKDbContext dbContext,
        ILogger<DatabaseSeeder> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        if (!IsAble())
        {
            return;
        }

        using var transaction = _dbContext.Database.BeginTransaction();
        _logger.LogInformation("Заполнение базы данных начато. Транзакция запущена.");
        try
        {
            _dbContext.Roles.AddRange(Roles.Values);
            _dbContext.Employees.Add(TestEmployee);
            await _dbContext.SaveChangesAsync();

            var result = await _userManager.CreateAsync(TestUser, TEST_PASSWORD);
            if (!result.Succeeded)
            {
                throw new Exception($"User was not added. {string.Join("\n", result.Errors.Select(e => $"{e.Code}.{e.Description}"))}");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(TestUser, DefaultRoles.Admin);
            if (!addToRoleResult.Succeeded)
            {
                throw new Exception($"User was not added to role. {string.Join("\n", addToRoleResult.Errors.Select(e => $"{e.Code}.{e.Description}"))}");
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            _logger.LogInformation("Произошла ошибка во время заполнения базы данных. Транзакция не была зафиксирована.");
            throw;
        }

        _logger.LogInformation("База данных была заполнена успешно. Транзакция зафиксирована.");
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
