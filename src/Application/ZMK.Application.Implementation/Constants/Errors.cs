using ZMK.Domain.Shared;

namespace ZMK.Application.Implementation.Constants;

public static class Errors
{
    public static class Auth
    {
        private const string BaseCode = "Auth";

        public static readonly Error InvalidUsernameOrPassword = new($"{BaseCode}.{nameof(InvalidUsernameOrPassword)}", "Неверный логин или пароль.");

        public static readonly Error Unauthorized = new($"{BaseCode}.{nameof(Unauthorized)}", "Войдите в аккаунт.");

        public static readonly Error AccessDenied = new($"{BaseCode}.{nameof(AccessDenied)}", "У вас недостаточно прав на выполнение этой операции.");
    }
    public static Error NotFound(string entityName) => new(nameof(NotFound), $"{entityName} с таким айди не найден в базе данных.");
}
