using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

/// <summary>
/// Сотрудник.
/// </summary>
public class Employee : Entity
{
    /// <summary>
    /// ФИО.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Должность.
    /// </summary>
    public string? Post { get; set; }

    /// <summary>
    /// Примечание.
    /// </summary>
    public string? Remark { get; set; }
}