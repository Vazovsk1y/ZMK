using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ZMK.Domain.Entities;

namespace ZMK.PostgresDAL.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Order).IsUnique();

        builder.HasIndex(e => e.Title).IsUnique();
    }
}