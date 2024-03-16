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

                    b.Property<double>("Count")
                        .HasColumnType("double precision");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MarkId")
                        .HasColumnType("uuid");

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
                            Id = new Guid("87a77114-06dc-4cbd-b581-45ae2025c1f2"),
                            Order = 1,
                            Title = "КМД"
                        },
                        new
                        {
                            Id = new Guid("0621633a-6719-4c79-9ffb-4df105042558"),
                            Order = 2,
                            Title = "ЛСБ"
                        },
                        new
                        {
                            Id = new Guid("82984460-10d1-424f-b9ef-f01e19443c3b"),
                            Order = 3,
                            Title = "Сборка"
                        },
                        new
                        {
                            Id = new Guid("301eda81-8248-4dd9-bad5-d1298c4932b2"),
                            Order = 4,
                            Title = "Сварка"
                        },
                        new
                        {
                            Id = new Guid("05580605-1707-49f2-a602-cbd7e02ccc66"),
                            Order = 5,
                            Title = "Зачистка"
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.CompleteEventEmployee", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EmployeeId")
                        .HasColumnType("uuid");

                    b.HasKey("EventId", "EmployeeId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("CompleteEventsEmployees");
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
                            Id = new Guid("07cb64ff-f58d-4641-9553-bca2e698ff27"),
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

                    b.Property<int>("Count")
                        .HasColumnType("integer");

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

                    b.Property<Guid?>("CreatorId")
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
                            Id = new Guid("9de646cd-80dd-4208-873e-2189ec3bf947"),
                            ConcurrencyStamp = "4023d295-5165-49bf-81f5-8c866584036f",
                            Description = "Администратор системы имеет право добавлять/изменять любые настройки и проэкты. Определяет текущую базу и ее местоположение.",
                            Name = "Администратор",
                            NormalizedName = "АДМИНИСТРАТОР"
                        },
                        new
                        {
                            Id = new Guid("996c6b35-d992-4765-879c-897d6cbd966a"),
                            ConcurrencyStamp = "8480c20c-461e-474f-b184-75070a094b20",
                            Description = "Пользователь имеет право вносить выполнение по маркам, создавать и изменять отгрузки.",
                            Name = "Пользователь",
                            NormalizedName = "ПОЛЬЗОВАТЕЛЬ"
                        },
                        new
                        {
                            Id = new Guid("89764bdb-ea0d-4c0f-a608-988051cc5e7f"),
                            ConcurrencyStamp = "9fe13121-8564-4333-9573-0ee54bcb4f88",
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
                            Id = new Guid("52907098-9737-4538-b88b-1be9d4ac9752"),
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "68a240ba-d2fb-484b-8b87-d00c73a5195a",
                            EmailConfirmed = false,
                            EmployeeId = new Guid("07cb64ff-f58d-4641-9553-bca2e698ff27"),
                            LockoutEnabled = true,
                            NormalizedUserName = "TESTADMIN",
                            PhoneNumberConfirmed = false,
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
                            UserId = new Guid("52907098-9737-4538-b88b-1be9d4ac9752"),
                            RoleId = new Guid("9de646cd-80dd-4208-873e-2189ec3bf947")
                        });
                });

            modelBuilder.Entity("ZMK.Domain.Entities.CompleteEvent", b =>
                {
                    b.HasBaseType("ZMK.Domain.Common.MarkEvent");

                    b.Property<Guid>("AreaId")
                        .HasColumnType("uuid");

                    b.HasIndex("AreaId");

                    b.ToTable("CompleteEvents");
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

            modelBuilder.Entity("ZMK.Domain.Entities.CompleteEventEmployee", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Entities.CompleteEvent", "CompleteEvent")
                        .WithMany("Executors")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CompleteEvent");

                    b.Navigation("Employee");
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

            modelBuilder.Entity("ZMK.Domain.Entities.Project", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity("ZMK.Domain.Entities.CompleteEvent", b =>
                {
                    b.HasOne("ZMK.Domain.Entities.Area", "Area")
                        .WithMany()
                        .HasForeignKey("AreaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZMK.Domain.Common.MarkEvent", null)
                        .WithOne()
                        .HasForeignKey("ZMK.Domain.Entities.CompleteEvent", "Id")
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

            modelBuilder.Entity("ZMK.Domain.Entities.CompleteEvent", b =>
                {
                    b.Navigation("Executors");
                });
#pragma warning restore 612, 618
        }
    }
}
