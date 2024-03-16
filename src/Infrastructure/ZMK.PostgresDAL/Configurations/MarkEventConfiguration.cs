using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Common;

namespace ZMK.PostgresDAL.Configurations;

internal class MarkEventConfiguration : IEntityTypeConfiguration<MarkEvent>
{
    public void Configure(EntityTypeBuilder<MarkEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.UseTptMappingStrategy();

        builder
            .Property(e => e.EventType)
            .HasConversion(e => e.ToString(), e => Enum.Parse<EventType>(e));

        builder.HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatorId);

        builder.HasOne(e => e.Mark)
            .WithMany()
            .HasForeignKey(e => e.MarkId);
    }
}
