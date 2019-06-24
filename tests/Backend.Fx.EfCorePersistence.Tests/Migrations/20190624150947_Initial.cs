using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Fx.EfCorePersistence.Tests.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bloggers",
                columns: table => new
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bloggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDemoTenant = table.Column<bool>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    DefaultCultureName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
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
                        name: "FK_Posts_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_BlogId",
                table: "Posts",
                column: "BlogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bloggers");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Blogs");
        }
    }
}
