﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ZMK.PostgresDAL;

#nullable disable

namespace ZMK.PostgresDAL.Migrations
{
    [DbContext(typeof(ZMKDbContext))]
    partial class ZMKDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("ZMK.Domain.Common.MarkEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MarkCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("MarkCount")
                        .HasColumnType("double precision");

                    b.Property<Guid>("MarkId")
                        .HasColumnType("uuid");

                    b.Property<int>("MarkOrder")
                        .HasColumnType("integer");

                    b.Property<string>("MarkTitle")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("MarkWeight")
                        .HasColumnType("double precision");

                    b.Property<string>("Remark")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("MarkId");

                    b.ToTable("MarksEvents");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Area", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<string>("Remark")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Order")
                        .IsUnique();

                    b.HasIndex("Title")
                        .IsUnique();

                    b.ToTable("Areas");

                    b.HasData(
                        new
                        {
                            Id = new Guid("adf9422b-1e6f-4c7a-a41d-759f806e9429"),
                            Order = 1,
                            Title = "КМД"
                        },
                        new
                        {
                            Id = new Guid("7c791c85-d09d-4ee6-89f9-056a21743434"),
                            Order = 2,
                            Title = "ЛСБ"
                        },
                        new
                        {
                            Id = new Guid("4083deec-6aa1-4072-ab9b-11524bc01ee5"),
                            Order = 3,
                            Title = "Сборка"
                        },
                        new
                        {
                            Id = new Guid("fd6f850e-14bb-462d-a9c5-1aca65424956"),
                            Order = 4,
                            Title = "Сварка"
                        },
                        new
                        {
                            Id = new Guid("28da9925-30b1-404f-be57-0224256e66ad"),
                            Order = 5,
                            Title = "Зачистка"
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Employee", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Post")
                        .HasColumnType("text");

                    b.Property<string>("Remark")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FullName")
                        .IsUnique();

                    b.ToTable("Employees");

                    b.HasData(
                        new
                        {
                            Id = new Guid("c1c8db59-00b0-4c00-8a79-0021b3b3d14e"),
                            FullName = "Тестовый Сотрудник",
                            Post = "Тестовый Сотрудник",
                            Remark = "Создан исключительно в целях тестирования, рекомендуется удалить."
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Mark", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Count")
                        .HasColumnType("double precision");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Weight")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId", "Code")
                        .IsUnique();

                    b.ToTable("Marks");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.MarkCompleteEventEmployee", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EmployeeId")
                        .HasColumnType("uuid");

                    b.HasKey("EventId", "EmployeeId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("MarkCompleteEventsEmployees");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ClosingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ContractNumber")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid");

                    b.Property<string>("Customer")
                        .HasColumnType("text");

                    b.Property<string>("FactoryNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Remark")
                        .HasColumnType("text");

                    b.Property<string>("Vendor")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("FactoryNumber")
                        .IsUnique();

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.ProjectArea", b =>
                {
                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AreaId")
                        .HasColumnType("uuid");

                    b.HasKey("ProjectId", "AreaId");

                    b.HasIndex("AreaId");

                    b.ToTable("ProjectsAreas");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.ProjectSettings", b =>
                {
                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<bool>("AllowMarksAdding")
                        .HasColumnType("boolean");

                    b.Property<bool>("AllowMarksDeleting")
                        .HasColumnType("boolean");

                    b.Property<bool>("AllowMarksModifying")
                        .HasColumnType("boolean");

                    b.Property<bool>("AreExecutorsRequired")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEditable")
                        .HasColumnType("boolean");

                    b.HasKey("ProjectId");

                    b.ToTable("ProjectsSettings");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("b2bc1ca1-7ba5-45b6-ab62-d00aea6177ba"),
                            ConcurrencyStamp = "fc4d41a0-e882-4226-978e-15f2da370356",
                            Description = "Администратор системы имеет право добавлять/изменять любые настройки и проэкты. Определяет текущую базу и ее местоположение.",
                            Name = "Администратор",
                            NormalizedName = "АДМИНИСТРАТОР"
                        },
                        new
                        {
                            Id = new Guid("94675b6e-f3ff-47bb-8532-f91e2e31e55f"),
                            ConcurrencyStamp = "dc929dae-0608-47b6-bc65-a8fef30492d5",
                            Description = "Пользователь имеет право вносить выполнение по маркам, создавать и изменять отгрузки.",
                            Name = "Пользователь",
                            NormalizedName = "ПОЛЬЗОВАТЕЛЬ"
                        },
                        new
                        {
                            Id = new Guid("96206f7f-8ea8-486d-ab3b-bd19d745c350"),
                            ConcurrencyStamp = "bc9d5de5-15d3-4e0a-8a72-4ec8552b416e",
                            Description = "Доступ к проэктам с правом просмотра данных.",
                            Name = "Читатель",
                            NormalizedName = "ЧИТАТЕЛЬ"
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ClosingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<Guid>("EmployeeId")
                        .HasColumnType("uuid");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("6416bea0-8a55-431a-8b8c-bb85f49375d8"),
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "8d43d803-db2e-4c34-b340-3ee1267b4375",
                            EmailConfirmed = false,
                            EmployeeId = new Guid("c1c8db59-00b0-4c00-8a79-0021b3b3d14e"),
                            LockoutEnabled = true,
                            NormalizedUserName = "TESTADMIN",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "2feb39da-0f6c-4319-8e2d-445bc5c013f7",
                            TwoFactorEnabled = false,
                            UserName = "TestAdmin"
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = new Guid("6416bea0-8a55-431a-8b8c-bb85f49375d8"),
                            RoleId = new Guid("b2bc1ca1-7ba5-45b6-ab62-d00aea6177ba")
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.MarkCompleteEvent", b =>
                {
                    b.HasBaseType("ZMK.Domain.Common.MarkEvent");

                    b.Property<Guid>("AreaId")
                        .HasColumnType("uuid");

                    b.Property<double>("CompleteCount")
                        .HasColumnType("double precision");

                    b.HasIndex("AreaId");

                    b.ToTable("MarkCompleteEvents");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ZMK.Domain.Common.MarkEvent", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Entities.Mark", "Mark")
                        .WithMany()
                        .HasForeignKey("MarkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");

                    b.Navigation("Mark");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Mark", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Project", "Project")
                        .WithMany("Marks")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.MarkCompleteEventEmployee", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Entities.MarkCompleteEvent", "MarkCompleteEvent")
                        .WithMany("Executors")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("MarkCompleteEvent");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Project", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.ProjectArea", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Area", "Area")
                        .WithMany("Projects")
                        .HasForeignKey("AreaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Entities.Project", "Project")
                        .WithMany("Areas")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Area");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.ProjectSettings", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Project", null)
                        .WithOne("Settings")
                        .HasForeignKey("ZMK.Domain.Entities.ProjectSettings", "ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Session", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.User", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.UserRole", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Entities.User", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.MarkCompleteEvent", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Area", "Area")
                        .WithMany()
                        .HasForeignKey("AreaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Common.MarkEvent", null)
                        .WithOne()
                        .HasForeignKey("ZMK.Domain.Entities.MarkCompleteEvent", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Area");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Area", b =>
                {
                    b.Navigation("Projects");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Project", b =>
                {
                    b.Navigation("Areas");

                    b.Navigation("Marks");

                    b.Navigation("Settings")
                        .IsRequired();
                });

            modelBuilder.Entity("ZMK.Domain.Entities.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.User", b =>
                {
                    b.Navigation("Roles");
                });

            modelBuilder.Entity("ZMK.Domain.Entities.MarkCompleteEvent", b =>
                {
                    b.Navigation("Executors");
                });
#pragma warning restore 612, 618
        }
    }
}
