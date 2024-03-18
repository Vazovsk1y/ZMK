using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Extensions;

public static class IQuryableEx
{
    public static async Task<Session?> LoadByIdAsync(this IQueryable<Session> sessions, Guid? id, CancellationToken cancellationToken = default)
    {
        return await sessions
            .Include(e => e.User)
            .ThenInclude(e => e!.Employee)
            .Include(e => e.User!.Roles)
            .ThenInclude(e => e.Role)
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}