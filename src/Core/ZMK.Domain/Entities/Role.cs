using Microsoft.AspNetCore.Identity;
using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Role : IdentityRole<Guid>, IHasId
{
    public override Guid Id { get => base.Id; }

    public IEnumerable<UserRole> Users { get; set; } = new List<UserRole>();

    /// <summary>
    /// Дополнительная информация о роли.
    /// </summary>
    public string? Description { get; set; }

    public Role()
    {
        Id = Guid.NewGuid();
    }
}
