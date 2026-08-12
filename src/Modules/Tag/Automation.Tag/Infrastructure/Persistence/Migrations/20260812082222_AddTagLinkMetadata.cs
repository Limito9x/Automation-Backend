using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Tag.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTagLinkMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagLinks_TagId_EntityType_EntityId",
                schema: "tag",
                table: "TagLinks");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "tag",
                table: "TagLinks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Metadata",
                schema: "tag",
                table: "TagLinks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "tag",
                table: "TagItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TagId",
                schema: "tag",
                table: "TagLinks",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagLinks_TagId",
                schema: "tag",
                table: "TagLinks");

            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "tag",
                table: "TagLinks");

            migrationBuilder.DropColumn(
                name: "Color",
                schema: "tag",
                table: "TagItems");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "tag",
                table: "TagLinks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TagId_EntityType_EntityId",
                schema: "tag",
                table: "TagLinks",
                columns: new[] { "TagId", "EntityType", "EntityId" },
                unique: true);
        }
    }
}

