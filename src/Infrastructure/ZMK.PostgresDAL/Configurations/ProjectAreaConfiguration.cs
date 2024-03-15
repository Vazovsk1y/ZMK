using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

public class ProjectAreaConfiguration : IEntityTypeConfiguration<ProjectArea>
{
    public void Configure(EntityTypeBuilder<ProjectArea> builder)
    {
        builder.HasKey(pa => new { pa.ProjectId, pa.AreaId });

        builder.HasOne(e => e.Area).WithMany(e => e.Projects);
        builder.HasOne(e => e.Project).WithMany(e => e.Areas);
    }
}