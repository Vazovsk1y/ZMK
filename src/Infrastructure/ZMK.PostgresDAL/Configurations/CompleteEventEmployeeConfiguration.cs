using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

internal class CompleteEventEmployeeConfiguration : IEntityTypeConfiguration<CompleteEventEmployee>
{
    public void Configure(EntityTypeBuilder<CompleteEventEmployee> builder)
    {
        builder.HasKey(e => new { e.EventId, e.EmployeeId });

        builder
            .HasOne(e => e.CompleteEvent)
            .WithMany(e => e.Executors)
            .HasForeignKey(e => e.EventId);

        builder
            .HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId);
    }
}
