﻿// <auto-generated />
using Backend.Fx.Environment.MultiTenancy;
using DemoBlog.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace DemoBlog.Mvc.Data.Application.Migrations
{
    [DbContext(typeof(BlogDbContext))]
    partial class BlogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Backend.Fx.Environment.MultiTenancy.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DefaultCultureName");

                    b.Property<string>("Description");

                    b.Property<bool>("IsDefault");

                    b.Property<bool>("IsDemoTenant");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("DemoBlog.Domain.Blog", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("BloggerId");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Description");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<int>("TenantId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("DemoBlog.Domain.Blogger", b =>
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

            modelBuilder.Entity("DemoBlog.Domain.Comment", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Author");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("Content");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<int>("InReplyToCommentId");

                    b.Property<int>("PostId");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Comment");
                });

            modelBuilder.Entity("DemoBlog.Domain.Post", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("BlogId");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("Content");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<int>("TenantId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("DemoBlog.Domain.Subscriber", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("BlogId");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ChangedOn");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Email");

                    b.Property<string>("Name");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Subscriber");
                });

            modelBuilder.Entity("DemoBlog.Domain.Comment", b =>
                {
                    b.HasOne("DemoBlog.Domain.Post", "Post")
                        .WithMany("Comments")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DemoBlog.Domain.Subscriber", b =>
                {
                    b.HasOne("DemoBlog.Domain.Blog", "Blog")
                        .WithMany("Subscribers")
                        .HasForeignKey("BlogId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
