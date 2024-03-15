using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

internal class MarkConfiguration : IEntityTypeConfiguration<Mark>
{
    public void Configure(EntityTypeBuilder<Mark> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasOne(e => e.Project).WithMany(e => e.Marks).HasForeignKey(e => e.ProjectId);
    }
}