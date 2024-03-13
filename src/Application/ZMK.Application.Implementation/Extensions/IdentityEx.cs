using Microsoft.AspNetCore.Identity;
using ZMK.Domain.Shared;

namespace ZMK.Application.Implementation.Extensions;

public static class IdentityEx
{
    public static IEnumerable<Error> ToErrors(this IEnumerable<IdentityError> identityErrors)
    {
        return identityErrors.Select(e => new Error($"Identity.{e.Code}", e.Description));
    }
}