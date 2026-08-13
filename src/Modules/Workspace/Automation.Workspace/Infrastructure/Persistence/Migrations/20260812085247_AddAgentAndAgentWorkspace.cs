using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentAndAgentWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RootPath",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.CreateTable(
                name: "Agents",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MachineKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RegistrationToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentWorkspaces",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalRootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AgentWorkspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentWorkspaces_Agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "resource",
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentWorkspaces_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "resource",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkspaces_AgentId_WorkspaceId",
                schema: "resource",
                table: "AgentWorkspaces",
                columns: new[] { "AgentId", "WorkspaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkspaces_WorkspaceId",
                schema: "resource",
                table: "AgentWorkspaces",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_MachineKey",
                schema: "resource",
                table: "Agents",
                column: "MachineKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_RegistrationToken",
                schema: "resource",
                table: "Agents",
                column: "RegistrationToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentWorkspaces",
                schema: "resource");

            migrationBuilder.DropTable(
                name: "Agents",
                schema: "resource");

            migrationBuilder.AddColumn<string>(
                name: "RootPath",
                schema: "resource",
                table: "Workspaces",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}

