using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Area : Entity
{
    public required string Title { get; set; }

    public required int Order { get; set; }

    public string? Remark { get; set; }
}