using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

public class ProjectSettingsConfiguration : IEntityTypeConfiguration<ProjectSettings>
{
    public void Configure(EntityTypeBuilder<ProjectSettings> builder)
    {
        builder.HasKey(ps => ps.ProjectId);
    }
}
