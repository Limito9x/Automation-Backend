using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Resource.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameStorageToWorkspaceAndEnrichResourceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceItems_Storages_StorageId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropTable(
                name: "Storages",
                schema: "resource");

            migrationBuilder.DropIndex(
                name: "IX_ResourceVersions_AssetId",
                schema: "resource",
                table: "ResourceVersions");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_AssetId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "AssetId",
                schema: "resource",
                table: "ResourceVersions");

            migrationBuilder.DropColumn(
                name: "AssetId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.RenameColumn(
                name: "StorageId",
                schema: "resource",
                table: "ResourceItems",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_ResourceItems_StorageId",
                schema: "resource",
                table: "ResourceItems",
                newName: "IX_ResourceItems_WorkspaceId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "resource",
                table: "ResourceVersions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContentId",
                schema: "resource",
                table: "ResourceItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                schema: "resource",
                table: "ResourceItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "resource",
                table: "ResourceItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformExtensionId",
                schema: "resource",
                table: "ResourceItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workspaces",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    RootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_ContentId",
                schema: "resource",
                table: "ResourceItems",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_PlatformExtensionId",
                schema: "resource",
                table: "ResourceItems",
                column: "PlatformExtensionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceItems_Workspaces_WorkspaceId",
                schema: "resource",
                table: "ResourceItems",
                column: "WorkspaceId",
                principalSchema: "resource",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceItems_Workspaces_WorkspaceId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropTable(
                name: "Workspaces",
                schema: "resource");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_ContentId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_PlatformExtensionId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "resource",
                table: "ResourceVersions");

            migrationBuilder.DropColumn(
                name: "ContentId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "FilePath",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "PlatformExtensionId",
                schema: "resource",
                table: "ResourceItems");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                schema: "resource",
                table: "ResourceItems",
                newName: "StorageId");

            migrationBuilder.RenameIndex(
                name: "IX_ResourceItems_WorkspaceId",
                schema: "resource",
                table: "ResourceItems",
                newName: "IX_ResourceItems_StorageId");

            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                schema: "resource",
                table: "ResourceVersions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                schema: "resource",
                table: "ResourceItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Storages",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersions_AssetId",
                schema: "resource",
                table: "ResourceVersions",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_AssetId",
                schema: "resource",
                table: "ResourceItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Storages_ProjectId_Name",
                schema: "resource",
                table: "Storages",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceItems_Storages_StorageId",
                schema: "resource",
                table: "ResourceItems",
                column: "StorageId",
                principalSchema: "resource",
                principalTable: "Storages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
