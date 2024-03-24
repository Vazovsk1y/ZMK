using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Project : Entity, IAuditable
{
    /// <summary>
    /// Заводской номер.
    /// </summary>
    public required string FactoryNumber { get; set; }

    /// <summary>
    /// Номер договора.
    /// </summary>
    public string? ContractNumber { get; set; }

    /// <summary>
    /// Заказчик.
    /// </summary>
    public string? Customer { get; set; }

    /// <summary>
    /// Поставщик.
    /// </summary>
    public string? Vendor { get; set; }

    public string? Remark { get; set; }

    public required Guid CreatorId { get; set; }

    /// <summary>
    /// Дата закрытия.
    /// </summary>
    public DateTimeOffset? ClosingDate { get; set; }

    public required DateTimeOffset CreatedDate { get; init; }

    public DateTimeOffset? ModifiedDate { get; set; }

    #region --Navigation--

    public User Creator { get; set; } = null!;

    public ProjectSettings Settings { get; set; } = null!;

    public IEnumerable<ProjectArea> Areas { get; set; } = new HashSet<ProjectArea>();

    public IEnumerable<Mark> Marks { get; set; } = new HashSet<Mark>();

    #endregion
}

public class ProjectSettings
{
    public required Guid ProjectId { get; set; }

    /// <summary>
    /// Можно ли редактировать проект.
    /// </summary>
    public bool IsEditable { get; set; }

    /// <summary>
    /// Разрешено ли удаление марок из проекта.
    /// </summary>
    public bool AllowMarksDeleting { get; set; }

    /// <summary>
    /// Разрешено ли редактирование марок из проекта.
    /// </summary>
    public bool AllowMarksModifying { get; set; }

    /// <summary>
    /// Разрешено ли добавление марок в проект.
    /// </summary>
    public bool AllowMarksAdding { get; set; }

    /// <summary>
    /// Проверять заполнителей при заполнении марки.
    /// </summary>
    public bool AreExecutorsRequired { get; set; }
}