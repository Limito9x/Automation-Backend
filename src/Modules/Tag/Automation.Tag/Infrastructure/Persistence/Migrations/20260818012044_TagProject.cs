using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Tag.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TagProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagGroups_Scope_Name",
                schema: "tag",
                table: "TagGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "tag",
                table: "TagLinks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "tag",
                table: "TagGroups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_ProjectId",
                schema: "tag",
                table: "TagLinks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TagGroups_ProjectId_Scope_Name",
                schema: "tag",
                table: "TagGroups",
                columns: new[] { "ProjectId", "Scope", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagLinks_ProjectId",
                schema: "tag",
                table: "TagLinks");

            migrationBuilder.DropIndex(
                name: "IX_TagGroups_ProjectId_Scope_Name",
                schema: "tag",
                table: "TagGroups");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "tag",
                table: "TagLinks");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "tag",
                table: "TagGroups");

            migrationBuilder.CreateIndex(
                name: "IX_TagGroups_Scope_Name",
                schema: "tag",
                table: "TagGroups",
                columns: new[] { "Scope", "Name" },
                unique: true);
        }
    }
}
