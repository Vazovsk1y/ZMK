namespace ZMK.Domain.Common;

public abstract class Entity : IHasId
{
    public Guid Id { get; } = Guid.NewGuid();
}