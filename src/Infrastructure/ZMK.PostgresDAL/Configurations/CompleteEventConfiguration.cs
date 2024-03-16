using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

internal class CompleteEventConfiguration : IEntityTypeConfiguration<CompleteEvent>
{
    public void Configure(EntityTypeBuilder<CompleteEvent> builder)
    {
        builder.UseTptMappingStrategy();

        builder
            .HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId);
    }
}