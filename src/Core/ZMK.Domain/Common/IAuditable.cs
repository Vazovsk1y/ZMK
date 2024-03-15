namespace ZMK.Domain.Common;

public interface IAuditable
{
    DateTimeOffset CreatedDate { get; }

    DateTimeOffset? ModifiedDate { get; set; }
}