using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceVersionFileHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                schema: "resource",
                table: "ResourceVersions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                schema: "resource",
                table: "ResourceVersions");
        }
    }
}

