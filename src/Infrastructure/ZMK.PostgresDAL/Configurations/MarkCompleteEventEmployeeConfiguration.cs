using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

internal class MarkCompleteEventEmployeeConfiguration : IEntityTypeConfiguration<MarkCompleteEventEmployee>
{
    public void Configure(EntityTypeBuilder<MarkCompleteEventEmployee> builder)
    {
        builder.HasKey(e => new { e.EventId, e.EmployeeId });

        builder
            .HasOne(e => e.MarkCompleteEvent)
            .WithMany(e => e.Executors)
            .HasForeignKey(e => e.EventId);

        builder
            .HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId);
    }
}
