using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

/// <summary>
/// Пользовательская сессия.
/// </summary>
public class Session : Entity
{
    public required Guid UserId { get; init; }
      
    public required DateTimeOffset CreationDate { get; set; }

    public DateTimeOffset? ClosingDate { get; set; }

    public bool IsActive { get; set; }

    #region --Navigation--

    public User User { get; set; } = null!;

    #endregion
}