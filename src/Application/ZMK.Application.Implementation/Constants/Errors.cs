using ZMK.Domain.Shared;

namespace ZMK.Application.Implementation.Constants;

public static class Errors
{
    public static class Auth
    {
        private const string BaseCode = "Auth";

        public static readonly Error InvalidUsernameOrPassword = new($"{BaseCode}.{nameof(InvalidUsernameOrPassword)}", "Неверный логин или пароль.");

        public static readonly Error SessionIsAlreadyOpened = new($"{BaseCode}.{nameof(SessionIsAlreadyOpened)}", "Другой пользователь уже использует приложение.");
    }
}
