using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl;

namespace Backend.Fx.EfCorePersistence.Tests.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("20170527113029_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Name");

                    b.Property<int>("TenantId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Blogger", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Bio");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<int>("TenantId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.ToTable("Bloggers");
                });

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BlogId");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Name");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Post");
                });

            modelBuilder.Entity("Backend.Fx.Environment.MultiTenancy.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDemoTenant");

                    b.Property<bool>("IsInitialized");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Post", b =>
                {
                    b.HasOne("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
