using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .HasIndex(e => e.FactoryNumber)
            .IsUnique();

        builder.HasOne(p => p.Creator)
            .WithMany()
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(e => e.Areas)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId);

        builder.HasOne(p => p.Settings)
            .WithOne()
            .HasForeignKey<ProjectSettings>(ps => ps.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}