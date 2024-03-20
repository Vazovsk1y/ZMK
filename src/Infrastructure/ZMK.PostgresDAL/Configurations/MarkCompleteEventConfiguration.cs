using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

internal class MarkCompleteEventConfiguration : IEntityTypeConfiguration<MarkCompleteEvent>
{
    public void Configure(EntityTypeBuilder<MarkCompleteEvent> builder)
    {
        builder.UseTptMappingStrategy();

        builder
            .HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId);
    }
}