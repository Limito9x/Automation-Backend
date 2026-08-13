using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceAgentAndResourceVersionLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspaces_AgentId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "AgentId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "RootPath",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.CreateTable(
                name: "WorkspaceAgents",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_WorkspaceAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceAgents_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "resource",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceVersionLocations",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsOrigin = table.Column<bool>(type: "boolean", nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ResourceVersionLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceVersionLocations_ResourceVersions_ResourceVersionId",
                        column: x => x.ResourceVersionId,
                        principalSchema: "resource",
                        principalTable: "ResourceVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceVersionLocations_WorkspaceAgents_WorkspaceAgentId",
                        column: x => x.WorkspaceAgentId,
                        principalSchema: "resource",
                        principalTable: "WorkspaceAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersions_ResourceId_FileHash",
                schema: "resource",
                table: "ResourceVersions",
                columns: new[] { "ResourceId", "FileHash" },
                unique: true,
                filter: "\"FileHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_ResourceVersionId",
                schema: "resource",
                table: "ResourceVersionLocations",
                column: "ResourceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId_RelativePath",
                schema: "resource",
                table: "ResourceVersionLocations",
                columns: new[] { "WorkspaceAgentId", "RelativePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceAgents_WorkspaceId_AgentId",
                schema: "resource",
                table: "WorkspaceAgents",
                columns: new[] { "WorkspaceId", "AgentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceVersionLocations",
                schema: "resource");

            migrationBuilder.DropTable(
                name: "WorkspaceAgents",
                schema: "resource");

            migrationBuilder.DropIndex(
                name: "IX_ResourceVersions_ResourceId_FileHash",
                schema: "resource",
                table: "ResourceVersions");

            migrationBuilder.AddColumn<Guid>(
                name: "AgentId",
                schema: "resource",
                table: "Workspaces",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                schema: "resource",
                table: "Workspaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformId",
                schema: "resource",
                table: "Workspaces",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RootPath",
                schema: "resource",
                table: "Workspaces",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_AgentId",
                schema: "resource",
                table: "Workspaces",
                column: "AgentId");
        }
    }
}
