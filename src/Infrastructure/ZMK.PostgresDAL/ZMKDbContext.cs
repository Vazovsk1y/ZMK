using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ZMK.Domain.Entities;
using ZMK.PostgresDAL.Extensions;

namespace ZMK.PostgresDAL;

public class ZMKDbContext(DbContextOptions options) : IdentityDbContext<
        User, Role, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<Employee> Employees { get; set; }

    public DbSet<Session> Sessions { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<ProjectSettings> ProjectsSettings { get; set; }

    public DbSet<Area> Areas { get; set; }

    public DbSet<ProjectArea> ProjectsAreas { get; set; }

    public DbSet<Mark> Marks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Seed();
    }
}