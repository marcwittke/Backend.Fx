using System;
using Microsoft.EntityFrameworkCore.Migrations;

// ReSharper disable RedundantArgumentDefaultValue

namespace Backend.Fx.EfCorePersistence.Tests.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Bloggers",
                table => new
                {
                    Id = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    LastName = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    Bio = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Bloggers", x => x.Id); });

            migrationBuilder.CreateTable(
                "Blogs",
                table => new
                {
                    Id = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Blogs", x => x.Id); });

            migrationBuilder.CreateTable(
                "Tenants",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                              .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDemoTenant = table.Column<bool>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    DefaultCultureName = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Tenants", x => x.Id); });

            migrationBuilder.CreateTable(
                "Posts",
                table => new
                {
                    Id = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ChangedOn = table.Column<DateTime>(nullable: true),
                    ChangedBy = table.Column<string>(maxLength: 100, nullable: true),
                    BlogId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    TargetAudience_Culture = table.Column<string>(nullable: true),
                    TargetAudience_IsPublic = table.Column<bool>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        "FK_Posts_Blogs_BlogId",
                        x => x.BlogId,
                        "Blogs",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_Posts_BlogId",
                "Posts",
                "BlogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Bloggers");

            migrationBuilder.DropTable(
                "Posts");

            migrationBuilder.DropTable(
                "Tenants");

            migrationBuilder.DropTable(
                "Blogs");
        }
    }
}