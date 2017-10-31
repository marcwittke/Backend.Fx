using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DemoBlog.Mvc.Data.Application.Migrations
{
    public partial class InitialSchemaCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DefaultCultureName = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false),
                    IsDemoTenant = table.Column<bool>(nullable: false),
                    IsInitialized = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    BloggerId = table.Column<int>(nullable: false),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bloggers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Bio = table.Column<string>(nullable: true),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bloggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    BlogId = table.Column<int>(nullable: false),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriber",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    BlogId = table.Column<int>(nullable: false),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriber_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Author = table.Column<string>(nullable: true),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    InReplyToCommentId = table.Column<int>(nullable: false),
                    PostId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PostId",
                table: "Comment",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriber_BlogId",
                table: "Subscriber",
                column: "BlogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Bloggers");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Subscriber");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Blogs");
        }
    }
}
