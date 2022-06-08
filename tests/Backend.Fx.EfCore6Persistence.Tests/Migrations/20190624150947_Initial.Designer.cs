﻿// <auto-generated />
using System;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Fx.EfCorePersistence.Tests.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("20190624150947_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.Blog", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Name");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<int>("TenantId");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.Blogger", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Bio");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<int>("TenantId");

                    b.HasKey("Id");

                    b.ToTable("Bloggers");
                });

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.Post", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("BlogId");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Name");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("Backend.Fx.Environment.MultiTenancy.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DefaultCultureName");

                    b.Property<string>("Description");

                    b.Property<bool>("IsDemoTenant");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.Post", b =>
                {
                    b.HasOne("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.TargetAudience", "TargetAudience", b1 =>
                        {
                            b1.Property<int?>("PostId");

                            b1.Property<string>("Culture");

                            b1.Property<bool>("IsPublic");

                            b1.ToTable("Posts");

                            b1.HasOne("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.Post")
                                .WithOne("TargetAudience")
                                .HasForeignKey("Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain.TargetAudience", "PostId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
