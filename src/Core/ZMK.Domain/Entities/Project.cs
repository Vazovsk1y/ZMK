﻿using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Project : Entity, IAuditable
{
    public required string FactoryNumber { get; set; }

    public string? ContractNumber { get; set; }

    public string? Customer { get; set; }

    public string? Vendor { get; set; }

    public string? Remark { get; set; }

    public required Guid? CreatorId { get; set; }

    public DateTimeOffset? ClosingDate { get; set; }

    public required DateTimeOffset CreatedDate { get; init; }

    public DateTimeOffset? ModifiedDate { get; set; }

    #region --Navigation--

    public User? Creator { get; set; }

    public ProjectSettings Settings { get; set; } = null!;

    #endregion
}

public class ProjectSettings
{
    public required Guid ProjectId { get; set; }

    public bool IsEditable { get; set; }

    public bool AllowMarksDeleting { get; set; }

    public bool AllowMarksModifying { get; set; }

    public bool AreExecutorsRequired { get; set; }
}