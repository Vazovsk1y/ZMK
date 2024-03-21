using Microsoft.AspNetCore.Identity;
using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class User : IdentityUser<Guid>, IHasId
{
    public override Guid Id { get => base.Id; }

    /// <summary>
    /// Ссылка на привязанного к пользователю сотрудника.
    /// </summary>
    public required Guid EmployeeId { get; set; }

    #region --Navigations--

    public IEnumerable<UserRole> Roles { get; set; } = new HashSet<UserRole>();

    public Employee Employee { get; set; } = null!;

    #endregion

    public User() 
    {
        Id = Guid.NewGuid();
    }
}