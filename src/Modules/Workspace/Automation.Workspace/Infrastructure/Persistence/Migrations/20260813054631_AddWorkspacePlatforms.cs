using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspacePlatforms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkspacePlatforms",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_WorkspacePlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspacePlatforms_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "resource",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspacePlatforms_WorkspaceId_PlatformId",
                schema: "resource",
                table: "WorkspacePlatforms",
                columns: new[] { "WorkspaceId", "PlatformId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspacePlatforms",
                schema: "resource");
        }
    }
}
